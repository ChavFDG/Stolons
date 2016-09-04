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

        public static void ManageBills(ApplicationDbContext dbContext)
        {
            ApplicationConfig.Modes lastMode = Configurations.Mode;
            do
            {
                ApplicationConfig.Modes currentMode = Configurations.Mode;
                if (lastMode == ApplicationConfig.Modes.Order && currentMode == ApplicationConfig.Modes.DeliveryAndStockUpdate)
                {
                    //We moved form Order to Preparation, create and send bills
                    List<ConsumerBill> consumerBills = new List<ConsumerBill>();
                    List<ProducerBill> producerBills = new List<ProducerBill>();
                    Dictionary<Producer, List<BillEntryConsumer>> brutProducerBills = new Dictionary<Producer, List<BillEntryConsumer>>();

                    #region Create bills
                    //Consumer (create bills)
                    List<ValidatedWeekBasket> consumerWeekBaskets = dbContext.ValidatedWeekBaskets.Include(x => x.Products).Include(x => x.Consumer).ToList();
                    StolonsBill stolonsBill = GenerateBill(consumerWeekBaskets, dbContext);
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
                            var billEntry = dbContext.BillEntrys.Include(x => x.Product).ThenInclude(x => x.Producer).First(x=>x.Id == tmpBillEntry.Id);
                            Producer producer = billEntry.Product.Producer;
                            if (!brutProducerBills.ContainsKey(producer))
                            {
                                brutProducerBills.Add(producer, new List<BillEntryConsumer>());
                            }
                            brutProducerBills[producer].Add(new BillEntryConsumer(billEntry,weekBasket.Consumer));
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
                    //Move product to validate
                    dbContext.Products.ToList().ForEach(x => x.State = Product.ProductState.Stock);

                    #if (DEBUG)
                        //For test, remove existing consumer bill and producer bill => That will never exit in normal mode cause they can only have one bill by week per user
                        dbContext.RemoveRange(dbContext.ConsumerBills.Where(x => consumerBills.Any(y => y.BillNumber == x.BillNumber)));
                        dbContext.RemoveRange(dbContext.ProducerBills.Where(x => producerBills.Any(y => y.BillNumber == x.BillNumber)));
                        dbContext.RemoveRange(dbContext.StolonsBills.Where(x => x.BillNumber == stolonsBill.BillNumber));
                     #endif
                    //
                    dbContext.SaveChanges();
                    //Set product remaining stock to week stock value
                    dbContext.Products.ToList().ForEach(x => x.RemainingStock = x.WeekStock);
                    dbContext.SaveChanges();
                    #endregion Save bills

                    #region Create PDF and send mail
                    //For stolons
                    string billWebAddress = Path.Combine("http://", Configurations.SiteUrl, "WeekBasketManagement", "ShowStolonsBill", stolonsBill.BillNumber).Replace("\\", "/");
                    GeneratePDF(billWebAddress, stolonsBill.FilePath);
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
                if (lastMode == ApplicationConfig.Modes.DeliveryAndStockUpdate && currentMode == ApplicationConfig.Modes.Order)
                {
                    foreach( var product in dbContext.Products.Where(x => x.State == Product.ProductState.Stock))
                    {
                        product.State = Product.ProductState.Disabled;
                    }
                    dbContext.SaveChanges();
                }
                lastMode = currentMode;
                Thread.Sleep(5000);
            } while (true);
        }

        private static void GeneratePdfAndSendEmail(ProducerBill bill)
        {
            //Generate pdf file
            GeneratePDF(bill);
            int cpt = 0;
            while(!File.Exists(bill.GetFilePath()))
            {
                cpt++;
                System.Threading.Thread.Sleep(500);
                if (cpt == 20)
                    return;
            }
            //Send mail to producer
            AuthMessageSender.SendEmail(bill.Producer.Email,
                                            bill.Producer.CompanyName,
                                            "Votre commande de la semaine (Facture " + bill.BillNumber + ")",
                                            "<h3>En pièce jointe votre commande de la semaine (Facture " + bill.BillNumber + ")</h3>",
                                            File.ReadAllBytes(bill.GetFilePath()),
                                            "Facture " + bill.GetFileName());

        }
        private static void GeneratePdfAndSendEmail(ConsumerBill bill)
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
            string message = "<h3>" + Configurations.ApplicationConfig.OrderDeliveryMessage + "</h3>";
            message += "<br/>";
            message += "<h4>En pièce jointe votre commande de la semaine (Facture " + bill.BillNumber + ")</h4>";

            AuthMessageSender.SendEmail(bill.User.Email,
                                            bill.User.Name,
                                            "Votre commande de la semaine (Facture " + bill.BillNumber + ")",
                                            message,
                                            File.ReadAllBytes(bill.GetFilePath()),
                                            "Facture " + bill.GetFileName());

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


        private static StolonsBill GenerateBill(List<ValidatedWeekBasket> consumerWeekBaskets, ApplicationDbContext dbContext)
        {
            StringBuilder builder = new StringBuilder();
            string billNumber = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            StolonsBill bill = new StolonsBill(billNumber);
            bill.Amount = 0;
            bill.Fee = Configurations.ApplicationConfig.Fee;
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
                    builder.AppendLine("<h2>Adhérent : "+weekBasket.Consumer.Id+ " / " + weekBasket.Consumer.Surname + " / " + weekBasket.Consumer.Name);
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
            bill.HtmlContent = builder.ToString();
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
            StringBuilder builder = new StringBuilder();
            //Calcul total amount
            decimal totalAmount = 0;
            foreach (var billEntry in billEntries)
            {
                totalAmount += Convert.ToDecimal(billEntry.BillEntry.Price * billEntry.BillEntry.Quantity);
            }
            bill.HtmlContent = builder.ToString();
            bill.Amount = totalAmount;
            bill.Fee = Configurations.ApplicationConfig.Fee;

            //Entete de facture
            builder.AppendLine("<h2>Facture : " + bill.BillNumber + "</h2>");
            builder.AppendLine("<p>Producteur : " + producer.CompanyName + "<p>");
            builder.AppendLine("<p>Année : " + DateTime.Now.Year);
            builder.AppendLine("<p>Semaine : " + DateTime.Now.GetIso8601WeekOfYear());

            #region Par produit
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

            builder.AppendLine("<h2>Commande par produit</h2>");

            builder.AppendLine("<table class=\"table\">");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Produit</th>");
            builder.AppendLine("<th>Prix unitaire</th>");
            builder.AppendLine("<th>Quantité</th>");
            builder.AppendLine("<th>Prix total</th>");
            builder.AppendLine("</tr>");
            foreach (var product in products)
            {
                int quantity = 0;
                product.Value.ForEach(x => quantity += x.BillEntry.Quantity);
                builder.AppendLine("<tr>");
                builder.AppendLine("<td>" + product.Key.Name + "</td>");
                builder.AppendLine("<td>" + product.Key.UnitPrice + " €" + "</td>");
                builder.AppendLine("<td>" + product.Key.GetQuantityString(quantity) + "</td>");
                builder.AppendLine("<td>" + Convert.ToDecimal(product.Key.UnitPrice * quantity) + " €" + "</td>");
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            builder.AppendLine("<p>Total sans comission : " + totalAmount + " €</p>");
            builder.AppendLine("<p>Comission (" + Configurations.ApplicationConfig.Fee + "%) : " + String.Format("{0:0.00}", bill.FeeAmount) + " €</p>");
            builder.AppendLine("<p>Total avec comission : " + String.Format("{0:0.00}", bill.ProducerAmount) + " €</p>");

            #endregion Par produit

            #region Par client
            builder.AppendLine("<h2>Commande par client</h2>");

            var billEntriesByConsumer = billEntries.GroupBy(x => x.Consumer);
            builder.AppendLine("<table class=\"table\">");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Client</th>");
            builder.AppendLine("<th>Produit</th>");
            builder.AppendLine("<th>Quantité</th>");
            builder.AppendLine("</tr>");
            foreach (var group in billEntriesByConsumer.OrderBy(x => x.Key.Id))
            {
                builder.AppendLine("<tr>");
                builder.AppendLine("<td colspan=\"3\" style=\"border-top:1px solid;\">" + "<b>" + group.Key.Id + "</b>" + "</td>");
                builder.AppendLine("</tr>");
                foreach (var entries in group.OrderBy(x => x.BillEntry.Product.Name))
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<td></td>");
                    builder.AppendLine("<td>" + entries.BillEntry.Product.Name + "</td>");
                    builder.AppendLine("<td>" + entries.BillEntry.QuantityString + "</td>");
                    builder.AppendLine("</tr>");
                }
            }
            builder.AppendLine("</table>");

            #endregion Par client
            bill.HtmlContent = builder.ToString();
            return bill;            
        }

        //CONSUMER BILL
        private static ConsumerBill GenerateBill(ValidatedWeekBasket weekBasket, ApplicationDbContext dbContext)
        {
            ConsumerBill bill = CreateBill<ConsumerBill>(weekBasket.Consumer);
            StringBuilder builder = new StringBuilder();

            //Calcul bill amount
            decimal total = 0;
            foreach (var billEntry in weekBasket.Products)
            {
                total += Convert.ToDecimal(billEntry.Price * billEntry.Quantity);
            }
            bill.Amount = total;

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
                builder.AppendLine("<tr>");
                builder.AppendLine("<td>" + billEntry.Product.Name + "</td>");
                builder.AppendLine("<td>" + billEntry.Product.UnitPrice + " €" + "</td>");
                builder.AppendLine("<td>" + billEntry.QuantityString + "</td>");
                builder.AppendLine("<td>" + Convert.ToDecimal(billEntry.Product.UnitPrice * billEntry.Quantity) + " €" + "</td>");
                builder.AppendLine("</tr>");
            }
            builder.AppendLine("</table>");
            builder.AppendLine("<p>Montant total : " + bill.Amount + " €</p>");
            bill.HtmlContent = builder.ToString();
            return bill;
        }

        private static T CreateBill<T>(User user) where T : class, IBill , new()
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
