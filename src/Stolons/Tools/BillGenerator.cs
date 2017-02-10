using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Stolons.Models;
using System.Threading;
using System.Globalization;
using Stolons.Helpers;
using Stolons.Services;
using Microsoft.EntityFrameworkCore;
using Stolons.Models.Users;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using static Stolons.Models.Product;

namespace Stolons.Tools
{
    public static  class BillGenerator
    {
        public class BillEntryConsumer
        {
            public BillEntry BillEntry { get; set; }
            public Consumer Consumer { get; set; }

            public BillEntryConsumer(BillEntry billEntry, Consumer consumer)
            {
                BillEntry = billEntry;
                Consumer = consumer;
            }
        }

        private static Dictionary<Guid, Stolon.Modes> lastModes = new Dictionary<Guid, Stolon.Modes>();

        public static void ManageBills(ApplicationDbContext dbContext)
        {
            foreach (Stolon stolon in dbContext.Stolons)
            {
                lastModes.Add(stolon.Id, stolon.GetMode());
            }
            do
            {
                foreach(Stolon stolon in dbContext.Stolons)
                {
                    Stolon.Modes currentMode = stolon.GetMode();
                    if (lastModes[stolon.Id] == Stolon.Modes.Order && currentMode == Stolon.Modes.DeliveryAndStockUpdate)
                    {
                        //We moved form Order to Preparation, create and send bills
                        List<ConsumerBill> consumerBills = new List<ConsumerBill>();
                        List<ProducerBill> producerBills = new List<ProducerBill>();
                        Dictionary<Producer, List<BillEntryConsumer>> brutProducerBills = new Dictionary<Producer, List<BillEntryConsumer>>();

                        #region Create bills
                        //Consumer (create bills)
                        List<ValidatedWeekBasket> consumerWeekBaskets = dbContext.ValidatedWeekBaskets.Include(x => x.Products).Include(x => x.Consumer).ToList();
                        StolonsBill stolonsBill = GenerateBill(stolon,consumerWeekBaskets, dbContext);
                        dbContext.Add(stolonsBill);
                        foreach (var weekBasket in consumerWeekBaskets)
                        {
                            //Generate bill for consumer
                            ConsumerBill consumerBill = GenerateBill(weekBasket, dbContext);
                            consumerBills.Add(consumerBill);
                            dbContext.Add(consumerBill);
                            //Add to producer bill entry
                            foreach (var tmpBillEntry in weekBasket.Products)
                            {
                                var billEntry = dbContext.BillEntrys.Include(x => x.Product).ThenInclude(x => x.Producer).First(x => x.Id == tmpBillEntry.Id);
                                Producer producer = billEntry.Product.Producer;
                                if (!brutProducerBills.ContainsKey(producer))
                                {
                                    brutProducerBills.Add(producer, new List<BillEntryConsumer>());
                                }
                                brutProducerBills[producer].Add(new BillEntryConsumer(billEntry, weekBasket.Consumer));
                            }
                        }
                        //Producer (creates bills)
                        foreach (var producerBill in brutProducerBills)
                        {
                            //Generate bill for producer
                            ProducerBill bill = GenerateBill(producerBill.Key, producerBill.Value, dbContext);
                            producerBills.Add(bill);
                            dbContext.Add(bill);
                        }
                        #endregion Create bills

                        #region Save bills
                        //Remove week basket
                        dbContext.TempsWeekBaskets.Clear();
                        dbContext.ValidatedWeekBaskets.Clear();
                        dbContext.BillEntrys.Clear();

                        //Move product to stock
                        dbContext.Products.ToList().Where(x => x.StockManagement == StockType.Week && x.State == ProductState.Enabled).ToList().ForEach(x => x.State = ProductState.Stock);

                        #if (DEBUG)
                            //For test, remove existing consumer bill and producer bill => That will never exist in normal mode cause they can only have one bill by week per user
                            dbContext.RemoveRange(dbContext.ConsumerBills.Where(x => consumerBills.Any(y => y.BillNumber == x.BillNumber)));
                            dbContext.RemoveRange(dbContext.ProducerBills.Where(x => producerBills.Any(y => y.BillNumber == x.BillNumber)));
                            dbContext.RemoveRange(dbContext.StolonsBills.Where(x => x.BillNumber == stolonsBill.BillNumber));
                        #endif
                        //
                        dbContext.SaveChanges();
                        //Set product remaining stock to week stock value
                        dbContext.Products.ToList().Where(x => x.StockManagement == StockType.Week).ToList().ForEach(x => x.RemainingStock = x.WeekStock);
                        dbContext.SaveChanges();
                        #endregion Save bills

                        #region Create PDF and send mail
                        //For stolons
                        string billWebAddress = Path.Combine("http://", Configurations.SiteUrl, "WeekBasketManagement", "ShowStolonsBill", stolonsBill.BillNumber).Replace("\\", "/");
                        try
                        {
                            GeneratePDF(billWebAddress, stolonsBill.FilePath);
                        }
                        catch (Exception exept)
                        {
                            AuthMessageSender.SendEmail(Configurations.Application.ContactMailAddress,
                                                            "Stolons",
                                                            "Erreur lors de la génération de la facture Stolons",
                                                            "Message d'erreur : " + exept.Message);
                        }

                        // => Producer, send mails
                        foreach (var bill in producerBills)
                        {
                            Thread thread = new Thread(() => GeneratePdfAndSendEmail(bill));
                            thread.Start();
                        }

                        //Bills (save bills and send mails to user)
                        foreach (var bill in consumerBills)
                        {
                            Thread thread = new Thread(() => GeneratePdfAndSendEmail(bill));
                            thread.Start();
                        }

                        #endregion  Create PDF and send mail
                    }
                    if (lastModes[stolon.Id] == Stolon.Modes.DeliveryAndStockUpdate && currentMode == Stolon.Modes.Order)
                    {
                        foreach (var product in dbContext.Products.Where(x => x.State == ProductState.Stock))
                        {
                            product.State = ProductState.Disabled;
                        }
                        dbContext.SaveChanges();
                    }
                    lastModes[stolon.Id] = currentMode;
                }                
                Thread.Sleep(5000);
            } while (true);
        }

        private static void GeneratePdfAndSendEmail(ProducerBill bill)
        {
            try
            {
                //Generate pdf file
                GeneratePDF(bill);
                int cpt = 0;
                while (!File.Exists(bill.GetFilePath()))
                {
                    cpt++;
                   Thread.Sleep(500);
                    if (cpt == 20)
                        return;
                }
                Thread.Sleep(50);
                //Send mail to producer
                AuthMessageSender.SendEmail(bill.Producer.Email,
                                                bill.Producer.CompanyName,
                                                "Votre commande de la semaine (Facture " + bill.BillNumber + ")",
                                                bill.HtmlOrderContent
                                                + "<h3>En pièce jointe votre facture de la semaine (Facture " + bill.BillNumber + ")</h3>",
                                                File.ReadAllBytes(bill.GetFilePath()),
                                                "Facture " + bill.GetFileName());
            }
            catch (Exception exept)
            {
                AuthMessageSender.SendEmail(Configurations.Application.ContactMailAddress,
                                                "Stolons",
                                                "Erreur lors de la génération de la facture " + bill.BillNumber + " à " + bill.Producer.Email,
                                                "Message d'erreur : " + exept.Message);
            }

        }
        private static void GeneratePdfAndSendEmail(ConsumerBill bill)
        {

            try
            {
                //Generate pdf file
                GeneratePDF(bill);
                int cpt = 0;
                while (!File.Exists(bill.GetFilePath()))
                {
                    cpt++;
                    System.Threading.Thread.Sleep(500);
                    if (cpt == 20)
                        return;
                }
                //Send mail to user with bill
                string message = "<h3>" + bill.User.Stolon.OrderDeliveryMessage + "</h3>";
                message += "<br/>";
                message += "<h4>En pièce jointe votre commande de la semaine (Facture " + bill.BillNumber + ")</h4>";
                if (bill.Consumer.Token > 0)
                    message += "<p>Vous avez " + bill.Consumer.Token + "𝞫, pensez à payer vos bogues lors de la récupération de votre commande.</p>";

                AuthMessageSender.SendEmail(bill.User.Email,
                                                bill.User.Name,
                                                "Votre commande de la semaine (Facture " + bill.BillNumber + ")",
                                                message,
                                                File.ReadAllBytes(bill.GetFilePath()),
                                                "Facture " + bill.GetFileName());
            }
            catch (Exception exept)
            {
                AuthMessageSender.SendEmail(Configurations.Application.ContactMailAddress,
                                                "Stolons",
                                                "Erreur lors de la génération de la facture " + bill.BillNumber + " à " + bill.User.Email,
                                                "Message d'erreur : " + exept.Message);
            }

        }

        private static string GetFilePath(this IBill bill)
        {
            return Path.Combine(Configurations.Environment.WebRootPath,
                                            bill.User is Producer ? Configurations.ProducersBillsStockagePath : Configurations.ConsumersBillsStockagePath,
                                            bill.User.Id.ToString(),
                                            bill.GetFileName());
        }
        private static string GetFileName(this IBill bill)
        {
            return  bill.BillNumber + ".pdf";
        }


        private static StolonsBill GenerateBill(Stolon stolon,List<ValidatedWeekBasket> consumerWeekBaskets, ApplicationDbContext dbContext)
        {
            StringBuilder builder = new StringBuilder();
            string billNumber = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            StolonsBill bill = new StolonsBill(billNumber);
            bill.Stolon = stolon;
            bill.Amount = 0;
            bill.ProducersFee =  stolon.ProducersFee;
            if (!consumerWeekBaskets.Any())
            {
                builder.AppendLine("Rien cette semaine !");
            }
            else
            {
                foreach (ValidatedWeekBasket tempWeekBasket in consumerWeekBaskets.OrderBy(x => x.Consumer.Id))
                {
                    ValidatedWeekBasket weekBasket = dbContext.ValidatedWeekBaskets.Include(x => x.Products).First(x => x.Id == tempWeekBasket.Id);
                    weekBasket.Products.ForEach(x => x = dbContext.BillEntrys.Include(b=>b.Product).First(b => b.Id == x.Id));
                    bill.Amount += weekBasket.TotalPrice;
                    //
                    builder.AppendLine("<h1>Adhérent : "+weekBasket.Consumer.Id+ " / " + weekBasket.Consumer.Surname + " / " + weekBasket.Consumer.Name +"</h1>");
                    builder.AppendLine("<p>Facture : " + billNumber+"_"+ weekBasket.Consumer.Id + "</p>");
                    builder.AppendLine("<p>Téléphone : " + weekBasket.Consumer.PhoneNumber + "</p>");
                    builder.AppendLine("<p>Total à régler : " + weekBasket.TotalPrice+ " €</p>");


                    //Create list of bill entry by product
                    Dictionary<Producer, List<BillEntry>> producersProducts = new Dictionary<Producer, List<BillEntry>>();
                    foreach (var billEntryConsumer in weekBasket.Products)
                    {
                        var billEntry = dbContext.BillEntrys.Include(x => x.Product).ThenInclude(x => x.Producer).First(x => x.Id == billEntryConsumer.Id);
                        if (!producersProducts.ContainsKey(billEntry.Product.Producer))
                        {
                            producersProducts.Add(billEntry.Product.Producer, new List<BillEntry>());
                        }
                        producersProducts[billEntry.Product.Producer].Add(billEntry);
                    }
                    List<int> rowsTotal = new List<int>();
                    // - Add products by producer
                    builder.AppendLine("<h2>Commande par producteur</h2>");
                    
                    builder.AppendLine("<table class=\"table\">");
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<th>Producteur</th>");
                    builder.AppendLine("<th>Produit</th>");
                    builder.AppendLine("<th>Quantité</th>");
                    builder.AppendLine("</tr>");

                    foreach (var producer in producersProducts.Keys.OrderBy(x => x.Id))
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine("<td colspan=\"3\" style=\"border-top:1px solid;\">" + "<b>" + producer.CompanyName + "</b>" + "</td>");
                        builder.AppendLine("</tr>");
                        foreach (var billEntry in producersProducts[producer].OrderBy(x => x.Product.Name))
                        {
                            builder.AppendLine("<tr>");
                            builder.AppendLine("<td></td>");
                            builder.AppendLine("<td>" + billEntry.Product.Name + "</td>");
                            builder.AppendLine("<td>" + billEntry.QuantityString + "</td>");
                            builder.AppendLine("</tr>");

                        }
                    }
                    builder.AppendLine("</table>");
                    builder.AppendLine("<divstyle=\"page-break-after:always;\">");
                }
            }
            bill.HtmlBillContent = builder.ToString();
            return bill;
        }

        /*
        *BILL NAME INFORMATION
        *Bills are stored like that : bills\UserId\Year_WeekNumber_UserId
        */

        //PRODUCER BILL
        private static ProducerBill GenerateBill(Producer producer, List<BillEntryConsumer> billEntries, ApplicationDbContext dbContext)
        {
            //Create bill
            ProducerBill bill = CreateBill<ProducerBill>(producer);
            //Calcul total amount
            decimal totalAmount = 0;
            foreach (var billEntry in billEntries)
            {
                totalAmount += billEntry.BillEntry.Price;
            }
            bill.OrderAmount = totalAmount;
            bill.ProducersFee = producer.Stolon.ProducersFee;
            //Create list of bill entry by product
            Dictionary<Product, List<BillEntryConsumer>> products = new Dictionary<Product, List<BillEntryConsumer>>();
            foreach (var billEntryConsumer in billEntries)
            {
                if (!products.ContainsKey(billEntryConsumer.BillEntry.Product))
                {
                    products.Add(billEntryConsumer.BillEntry.Product, new List<BillEntryConsumer>());
                }
                products[billEntryConsumer.BillEntry.Product].Add(billEntryConsumer);
            }

            //GENERATION FACTURE
            StringBuilder billBuilder = new StringBuilder();
            //Entete de facture
            //  Producteur
            billBuilder.AppendLine("<p>"+producer.CompanyName + "<p>");
            billBuilder.AppendLine("<p>" + producer.Surname?.ToUpper()+ " " + producer.Name?.ToUpper() +"<p>");
            billBuilder.AppendLine("<p>" + producer.Address+"</p>");
            billBuilder.AppendLine("<p>" + producer.PostCode +" " +producer.City?.ToUpper()+ "</p>");
            billBuilder.AppendLine("<br>");
            billBuilder.AppendLine("<p>Facture n° " + bill.BillNumber + "</p>");
            billBuilder.AppendLine("<p>Année : " + DateTime.Now.Year);
            billBuilder.AppendLine("<p>Semaine : " + DateTime.Now.GetIso8601WeekOfYear());
            //  Destinataire

            //
            
            billBuilder.AppendLine("<br>");
            billBuilder.AppendLine("<table class=\"table\">");
            billBuilder.AppendLine("<tr>");
            billBuilder.AppendLine("<th>Produit</th>");
            billBuilder.AppendLine("<th>Quantité</th>");
            billBuilder.AppendLine("<th>TVA</th>");
            billBuilder.AppendLine("<th>PU HT</th>");
            billBuilder.AppendLine("<th>TOTAL HT</th>");
            billBuilder.AppendLine("</tr>");
            //Taux tax / Total HT
            Dictionary<decimal, decimal> taxTotal = new Dictionary<decimal, decimal>();
            decimal totalWithoutTax = 0;
            foreach (var product in products)
            {
                int quantity = 0;
                product.Value.ForEach(x => quantity += x.BillEntry.Quantity);
                decimal productTotalWithoutTax = Convert.ToDecimal(product.Key.UnitPriceWithoutFeeAndTax * quantity);
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td>" + product.Key.Name + "</td>");
                billBuilder.AppendLine("<td>" + product.Key.GetQuantityString(quantity) + "</td>");
                billBuilder.AppendLine("<td>" + (product.Key.TaxEnum == Product.TAX.None ? "NA": product.Key.Tax.ToString()) + " %</td>");
                billBuilder.AppendLine("<td>" + (product.Key.Type == SellType.Piece ? product.Key.UnitPriceWithoutFeeAndTax : product.Key.PriceWithoutFeeAndTax) + " €" + "</td>");
                billBuilder.AppendLine("<td>" + productTotalWithoutTax + " €" + "</td>");
                billBuilder.AppendLine("</tr>");
                //Si tax, on ajoute au total du taux de la tva
                if (product.Key.TaxEnum != Product.TAX.None)
                {
                    if (taxTotal.ContainsKey(product.Key.Tax))
                        taxTotal[product.Key.Tax]+= productTotalWithoutTax;
                    else
                        taxTotal.Add(product.Key.Tax, productTotalWithoutTax);
                }
                totalWithoutTax += productTotalWithoutTax;
            }
            billBuilder.AppendLine("<tr>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td>Total HT</td>");
            billBuilder.AppendLine("<td>"+ totalWithoutTax + " €</td>");
            billBuilder.AppendLine("</tr>");
            bill.TaxAmount = 0;
            foreach(var tax in taxTotal)
            {
                decimal taxAmount = Math.Round(tax.Value / 100m * tax.Key,2);
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td>TAV "+ tax.Key +"%</td>");
                billBuilder.AppendLine("<td>" + taxAmount + " €</td>");
                billBuilder.AppendLine("</tr>");
                bill.TaxAmount += taxAmount;
            }
            billBuilder.AppendLine("<tr>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td>Net à payer</td>");
            billBuilder.AppendLine("<td>" + bill.BillAmount + " €</td>");
            billBuilder.AppendLine("</tr>");
            billBuilder.AppendLine("</table>");
            
            bill.HtmlBillContent = billBuilder.ToString();









            //GENERATION COMMANDE
            StringBuilder orderBuilder = new StringBuilder();
            //Entete de facture
            //  Producteur
            orderBuilder.AppendLine("<h3> Commande n°" + bill.BillNumber + "</h3>");
            orderBuilder.AppendLine("<p>" + producer.CompanyName + "<p>");
            orderBuilder.AppendLine("<p>Année : " + DateTime.Now.Year);
            orderBuilder.AppendLine("<p>Semaine : " + DateTime.Now.GetIso8601WeekOfYear());
            orderBuilder.AppendLine("<br>");
            #region Par produit

            orderBuilder.AppendLine("<h3>Commande par produit</h3>");

            orderBuilder.AppendLine("<table class=\"table\">");
            orderBuilder.AppendLine("<tr>");
            orderBuilder.AppendLine("<th>Produit</th>");
            orderBuilder.AppendLine("<th>Quantité</th>");
            orderBuilder.AppendLine("</tr>");
            foreach (var product in products)
            {
                int quantity = 0;
                product.Value.ForEach(x => quantity += x.BillEntry.Quantity);
                orderBuilder.AppendLine("<tr>");
                orderBuilder.AppendLine("<td>" + product.Key.Name + "</td>");
                orderBuilder.AppendLine("<td>" + product.Key.GetQuantityString(quantity) + "</td>");
                orderBuilder.AppendLine("</tr>");
            }
            orderBuilder.AppendLine("</table>");

            #endregion Par produit

            #region Par client
            orderBuilder.AppendLine("<h3>Commande par client</h3>");

            var billEntriesByConsumer = billEntries.GroupBy(x => x.Consumer);
            orderBuilder.AppendLine("<table class=\"table\">");
            orderBuilder.AppendLine("<tr>");
            orderBuilder.AppendLine("<th>Client</th>");
            orderBuilder.AppendLine("<th>Produit</th>");
            orderBuilder.AppendLine("<th>Quantité</th>");
            orderBuilder.AppendLine("</tr>");
            foreach (var group in billEntriesByConsumer.OrderBy(x => x.Key.Id))
            {
                orderBuilder.AppendLine("<tr>");
                orderBuilder.AppendLine("<td colspan=\"3\" style=\"border-top:1px solid;\">" + "<b>" + group.Key.Id + "</b>" + "</td>");
                orderBuilder.AppendLine("</tr>");
                foreach (var entries in group.OrderBy(x => x.BillEntry.Product.Name))
                {
                    orderBuilder.AppendLine("<tr>");
                    orderBuilder.AppendLine("<td></td>");
                    orderBuilder.AppendLine("<td>" + entries.BillEntry.Product.Name + "</td>");
                    orderBuilder.AppendLine("<td>" + entries.BillEntry.QuantityString + "</td>");
                    orderBuilder.AppendLine("</tr>");
                }
            }
            orderBuilder.AppendLine("</table>");

            #endregion Par client
            bill.HtmlOrderContent = orderBuilder.ToString();
            return bill;            
        }

        //CONSUMER BILL
        private static ConsumerBill GenerateBill(ValidatedWeekBasket weekBasket, ApplicationDbContext dbContext)
        {
            ConsumerBill bill = CreateBill<ConsumerBill>(weekBasket.Consumer);
            StringBuilder builder = new StringBuilder();
            bill.OrderAmount = 0;

            //Entete de facture
            builder.AppendLine("<h2>Facture : " + bill.BillNumber + "</h2>");
            builder.AppendLine("<p>Numéro d'adhérent : " + weekBasket.Consumer.Id + "<p>");
            builder.AppendLine("<p>Nom : " + weekBasket.Consumer.Name + "<p>");
            builder.AppendLine("<p>Prénom : " + weekBasket.Consumer.Surname + "<p>");
            builder.AppendLine("<p>Téléphone : " + weekBasket.Consumer.PhoneNumber + "<p>");
            builder.AppendLine("<p>Année : " + DateTime.Now.Year);
            builder.AppendLine("<p>Semaine : " + DateTime.Now.GetIso8601WeekOfYear());
            //
            builder.AppendLine("<h2>Produits de votre panier de la semaine :</h2>");
            builder.AppendLine("<table class=\"table\">");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Produit</th>");
            builder.AppendLine("<th>Prix unitaire</th>");
            builder.AppendLine("<th>Quantité</th>");
            builder.AppendLine("<th>Prix total</th>");
            builder.AppendLine("</tr>");
            foreach (var tmpBillEntry in weekBasket.Products)
            {
                var billEntry = dbContext.BillEntrys.Include(x => x.Product).ThenInclude(x => x.Familly).First(x => x.Id == tmpBillEntry.Id);
                decimal total = Convert.ToDecimal(billEntry.Product.UnitPrice * billEntry.Quantity);
                builder.AppendLine("<tr>");
                builder.AppendLine("<td>" + billEntry.Product.Name + "</td>");
                builder.AppendLine("<td>" + billEntry.Product.UnitPrice + " €" + "</td>");
                builder.AppendLine("<td>" + billEntry.QuantityString + "</td>");
                builder.AppendLine("<td>" + total + " €" + "</td>");
                builder.AppendLine("</tr>");
                bill.OrderAmount += total;
            }
            builder.AppendLine("</table>");
            builder.AppendLine("<p>Montant total : " + bill.OrderAmount + " €</p>");
            bill.HtmlBillContent = builder.ToString();
            return bill;
        }

        private static T CreateBill<T>(StolonsUser user) where T : class, IBill , new()
        {
            IBill bill = new T();
            bill.BillNumber = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear() +"_" + user.Id;
            bill.User = user;
            bill.State = BillState.Pending;
            bill.EditionDate = DateTime.Now;
            return bill as T;
        }

        public static void GeneratePDF(IBill bill)
        {
            string billWebAddress = Path.Combine("http://",Configurations.SiteUrl ,"Bills","ShowBill" ,bill.BillNumber).Replace("\\", "/");
            GeneratePDF(billWebAddress, bill.GetFilePath());
        }

        /// <summary>
        /// Generate a pdf file from a web url to a file path
        /// </summary>
        /// <param name="webAddress"></param>
        /// <param name="filePath"></param>
        public static void GeneratePDF(string webAddress, string filePath)
        {

            while (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string phantomFolder = Path.Combine(Configurations.Environment.WebRootPath, "phantomjs");
            string phantomjs = Path.Combine(phantomFolder, "phantomjs.exe");
            string rasterizejs = Path.Combine(Configurations.Environment.WebRootPath, "phantomjs", "rasterize.js");

            //Création du PDF par phantomjs
            string arguments = "\"" + rasterizejs + "\" \"" + webAddress + "\" " + filePath;
            var proc = Process.Start(phantomjs, arguments);
        }

        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
        }
        
    }
}
