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
using MoreLinq;

namespace Stolons.Tools
{
    public static class BillGenerator
    {
        private static Dictionary<Guid, Stolon.Modes> lastModes = new Dictionary<Guid, Stolon.Modes>();

        public static void ManageBills()
        {
            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                foreach (Stolon stolon in dbContext.Stolons)
                {
                    lastModes.Add(stolon.Id, stolon.GetMode());
                }
            }
            do
            {
                ApplicationDbContext dbContext = new ApplicationDbContext();
                foreach (Stolon stolon in dbContext.Stolons)
                {
                    //For new Stolon
                    if (!lastModes.Keys.Contains(stolon.Id))
                        lastModes.Add(stolon.Id, stolon.GetMode());
                    //

                    Stolon.Modes currentMode = stolon.GetMode();
                    if (lastModes[stolon.Id] == Stolon.Modes.Order && currentMode == Stolon.Modes.DeliveryAndStockUpdate)
                    {
                        //We moved form Order to Preparation, create and send bills
                        List<ConsumerBill> consumerBills = new List<ConsumerBill>();
                        List<ProducerBill> producerBills = new List<ProducerBill>();

                        #region Create bills
                        //Consumer (create bills)
                        List<ValidatedWeekBasket> consumerWeekBaskets = dbContext.ValidatedWeekBaskets.Include(x => x.BillEntries).Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Stolon).Include(x => x.AdherentStolon.Adherent)
                                                                                                      .Where(x => x.AdherentStolon.StolonId == stolon.Id).ToList();
                        foreach (var weekBasket in consumerWeekBaskets)
                        {
                            //Generate bill for consumer                            
                            ConsumerBill consumerBill = CreateBill<ConsumerBill>(weekBasket.AdherentStolon, weekBasket.BillEntries);
                            consumerBill.HtmlBillContent = GenerateHtmlBillContent(consumerBill, dbContext);
                            consumerBills.Add(consumerBill);
                            dbContext.Add(consumerBill);
                        }
                        //Producer (creates bills)
                        foreach (var producer in dbContext.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).Where(x => x.StolonId == stolon.Id && x.IsProducer))
                        {
                            List<BillEntry> billEntries = new List<BillEntry>();
                            consumerBills.ForEach(consumerBill => consumerBill.BillEntries.Where(billEntry => billEntry.ProductStock.Product.ProducerId == producer.Id).ToList().ForEach(x => billEntries.Add(x)));
                            //Generate bill for producer
                            ProducerBill bill = CreateBill<ProducerBill>(producer, billEntries);
                            bill.HtmlBillContent = GenerateHtmlBillContent(bill, dbContext);
                            bill.HtmlOrderContent = GenerateHtmlOrderContent(bill, dbContext);
                            producerBills.Add(bill);
                            dbContext.Add(bill);
                        }
                        //Stolons
                        StolonsBill stolonsBill = GenerateBill(stolon, consumerWeekBaskets, dbContext);
                        dbContext.Add(stolonsBill);
                        #endregion Create bills

                        #region Save bills
                        //Remove week basket
                        dbContext.TempsWeekBaskets.Clear();
                        dbContext.ValidatedWeekBaskets.Clear();
                        dbContext.BillEntrys.Clear();

                        //Move stolon's products to stock 
                        foreach (ProductStockStolon productStock in dbContext.ProductsStocks.Include(x => x.AdherentStolon).Include(x => x.Product).Where(x => x.AdherentStolon.StolonId == stolon.Id))
                        {
                            if (productStock.State == ProductState.Enabled && productStock.Product.StockManagement == StockType.Week)
                            {
                                productStock.State = ProductState.Stock;
                                productStock.RemainingStock = productStock.WeekStock;
                            }
                        }
#if (DEBUG)
                        //For test, remove existing consumer bill and producer bill => That will never exist in normal mode cause they can only have one bill by week per user
                        dbContext.RemoveRange(dbContext.ConsumerBills.Where(x => consumerBills.Any(y => y.BillNumber == x.BillNumber)));
                        dbContext.RemoveRange(dbContext.ProducerBills.Where(x => producerBills.Any(y => y.BillNumber == x.BillNumber)));
                        dbContext.RemoveRange(dbContext.StolonsBills.Where(x => x.BillNumber == stolonsBill.BillNumber));
#endif
                        //
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
                            AuthMessageSender.SendEmail(stolon.Label,
                                                            Configurations.Application.ContactMailAddress,
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
                        foreach (ProductStockStolon productStock in dbContext.ProductsStocks.Include(x => x.AdherentStolon).Include(x => x.Product).Where(x => x.AdherentStolon.StolonId == stolon.Id))
                        {
                            if (productStock.State == ProductState.Stock)
                            {
                                productStock.State = ProductState.Disabled;
                            }
                        }
                        dbContext.SaveChanges();
                    }
                    lastModes[stolon.Id] = currentMode;
                }
                dbContext.Dispose();
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
                AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                                bill.Adherent.Email,
                                                bill.Adherent.CompanyName,
                                                "Votre commande de la semaine (Facture " + bill.BillNumber + ")",
                                                bill.HtmlOrderContent
                                                + "<h3>En pièce jointe votre facture de la semaine (Facture " + bill.BillNumber + ")</h3>",
                                                File.ReadAllBytes(bill.GetFilePath()),
                                                "Facture " + bill.GetFileName());
            }
            catch (Exception exept)
            {
                AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                                Configurations.Application.ContactMailAddress,
                                                "Stolons",
                                                "Erreur lors de la génération de la facture " + bill.BillNumber + " à " + bill.Adherent.Email,
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
                string message = "<h3>" + bill.AdherentStolon.Stolon.OrderDeliveryMessage + "</h3>";
                message += "<br/>";
                message += "<h4>En pièce jointe votre commande de la semaine (Facture " + bill.BillNumber + ")</h4>";
                if (bill.AdherentStolon.Token > 0)
                    message += "<p>Vous avez " + bill.AdherentStolon.Token + "𝞫, pensez à payer vos bogues lors de la récupération de votre commande.</p>";

                AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                                bill.Adherent.Email,
                                                bill.Adherent.Name,
                                                "Votre commande de la semaine (Facture " + bill.BillNumber + ")",
                                                message,
                                                File.ReadAllBytes(bill.GetFilePath()),
                                                "Facture " + bill.GetFileName());
            }
            catch (Exception exept)
            {
                AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                                Configurations.Application.ContactMailAddress,
                                                "Stolons",
                                                "Erreur lors de la génération de la facture " + bill.BillNumber + " à " + bill.Adherent.Email,
                                                "Message d'erreur : " + exept.Message);
            }

        }

        private static string GetFilePath(this IBill bill)
        {
            return Path.Combine(Configurations.Environment.WebRootPath,
                                            bill.AdherentStolon.Adherent is Adherent ? Configurations.ProducersBillsStockagePath : Configurations.ConsumersBillsStockagePath,
                                            bill.AdherentStolon.Id.ToString(),
                                            bill.GetFileName());
        }
        private static string GetFileName(this IBill bill)
        {
            return bill.BillNumber + ".pdf";
        }


        private static StolonsBill GenerateBill(Stolon stolon, List<ValidatedWeekBasket> consumerWeekBaskets, ApplicationDbContext dbContext)
        {
            StringBuilder builder = new StringBuilder();
            string billNumber = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            StolonsBill bill = new StolonsBill(billNumber);
            bill.Stolon = stolon;
            bill.Amount = 0;
            bill.ProducersFee = stolon.ProducersFee;

            if (!consumerWeekBaskets.Any())
            {
                builder.AppendLine("Rien cette semaine !");
            }
            else
            {
                /*
                foreach (ValidatedWeekBasket tempWeekBasket in consumerWeekBaskets.OrderBy(x => x.AdherentStolon.LocalId))
                {
                    ValidatedWeekBasket weekBasket = dbContext.ValidatedWeekBaskets.Include(x => x.BillEntries).First(x => x.Id == tempWeekBasket.Id);
                    weekBasket.BillEntries.ForEach(x => x = dbContext.BillEntrys.First(b => b.Id == x.Id));
                    bill.Amount += weekBasket.TotalPrice;
                    //
                    builder.AppendLine("<h1>Adhérent : " + weekBasket.AdherentStolon.LocalId + " / " + weekBasket.Adherent.Surname + " / " + weekBasket.Adherent.Name + "</h1>");
                    builder.AppendLine("<p>Facture : " + billNumber + "_" + weekBasket.Consumer.Id + "</p>");
                    builder.AppendLine("<p>Téléphone : " + weekBasket.Consumer.PhoneNumber + "</p>");
                    builder.AppendLine("<p>Total à régler : " + weekBasket.TotalPrice + " €</p>");


                    //Create list of bill entry by product
                    Dictionary<Adherent, List<BillEntry>> producersProducts = new Dictionary<Adherent, List<BillEntry>>();
                    foreach (var billEntryConsumer in weekBasket.BillEntries)
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
                */
            }
            bill.HtmlBillContent = builder.ToString();

            return bill;
        }

        //BILL NAME INFORMATION
        //Bills are stored like that : bills\UserId\Year_WeekNumber_UserId

        //PRODUCER BILL

        private static string GenerateHtmlOrderContent(ProducerBill bill, ApplicationDbContext dbContext)
        {
            //GENERATION COMMANDE
            StringBuilder orderBuilder = new StringBuilder();
            //Entete de facture
            //  Producteur
            orderBuilder.AppendLine("<h3> Commande n°" + bill.BillNumber + "</h3>");
            orderBuilder.AppendLine("<p>" + bill.Adherent.CompanyName + "<p>");
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

            foreach (var productBillEntries in bill.BillEntries.GroupBy(x => x.ProductStock.Product, x => x).OrderBy(x => x.Key.Name))
            {
                int quantity = 0;
                productBillEntries.ForEach(x => quantity += x.Quantity);
                orderBuilder.AppendLine("<tr>");
                orderBuilder.AppendLine("<td>" + productBillEntries.Key.Name + "</td>");
                orderBuilder.AppendLine("<td>" + productBillEntries.Key.GetQuantityString(quantity) + "</td>");
                orderBuilder.AppendLine("</tr>");
            }
            orderBuilder.AppendLine("</table>");

            #endregion Par produit

            #region Par client
            orderBuilder.AppendLine("<h3>Commande par client</h3>");

            var billEntriesByConsumer = bill.BillEntries.GroupBy(x => x.ConsumerBill.AdherentStolon);
            orderBuilder.AppendLine("<table class=\"table\">");
            orderBuilder.AppendLine("<tr>");
            orderBuilder.AppendLine("<th>Client</th>");
            orderBuilder.AppendLine("<th>Produit</th>");
            orderBuilder.AppendLine("<th>Quantité</th>");
            orderBuilder.AppendLine("</tr>");
            foreach (var group in billEntriesByConsumer.OrderBy(x => x.Key.LocalId))
            {
                orderBuilder.AppendLine("<tr>");
                orderBuilder.AppendLine("<td colspan=\"3\" style=\"border-top:1px solid;\">" + "<b>" + group.Key.Id + "</b>" + "</td>");
                orderBuilder.AppendLine("</tr>");
                foreach (var entries in group.OrderBy(x => x.ProductStock.ProductId))
                {
                    orderBuilder.AppendLine("<tr>");
                    orderBuilder.AppendLine("<td></td>");
                    orderBuilder.AppendLine("<td>" + entries.Name + "</td>");
                    orderBuilder.AppendLine("<td>" + entries.QuantityString + "</td>");
                    orderBuilder.AppendLine("</tr>");
                }
            }
            orderBuilder.AppendLine("</table>");

            #endregion Par client
            return orderBuilder.ToString();
        }

        private static string GenerateHtmlBillContent(ProducerBill bill, ApplicationDbContext dbContext)
        {
            //Calcul total amount
            decimal totalAmount = 0;
            foreach (var billEntry in bill.BillEntries)
            {
                totalAmount += billEntry.Price;
            }
            bill.OrderAmount = totalAmount;
            bill.ProducersFee = bill.AdherentStolon.Stolon.ProducersFee;

            //GENERATION FACTURE
            StringBuilder billBuilder = new StringBuilder();
            //Entete de facture
            //  Producteur
            billBuilder.AppendLine("<p>" + bill.Adherent.CompanyName + "<p>");
            billBuilder.AppendLine("<p>" + bill.Adherent.Surname?.ToUpper() + " " + bill.Adherent.Name?.ToUpper() + "<p>");
            billBuilder.AppendLine("<p>" + bill.Adherent.Address + "</p>");
            billBuilder.AppendLine("<p>" + bill.Adherent.PostCode + " " + bill.Adherent.City?.ToUpper() + "</p>");
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
            foreach (var productBillEntries in bill.BillEntries.GroupBy(x => x.ProductStock.Product, x => x).OrderBy(x => x.Key.Name))
            {
                int quantity = 0;
                productBillEntries.ForEach(x => quantity += x.Quantity);
                decimal productTotalWithoutTax = Convert.ToDecimal(productBillEntries.First().UnitPriceWithoutFeeAndTax * quantity);
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td>" + productBillEntries.Key.Name + "</td>");
                billBuilder.AppendLine("<td>" + productBillEntries.Key.GetQuantityString(quantity) + "</td>");
                billBuilder.AppendLine("<td>" + (productBillEntries.Key.TaxEnum == Product.TAX.None ? "NA" : productBillEntries.Key.Tax.ToString()) + " %</td>");
                billBuilder.AppendLine("<td>" + (productBillEntries.Key.Type == SellType.Piece ? productBillEntries.First().UnitPriceWithoutFeeAndTax : productBillEntries.First().PriceWithoutFeeAndTax) + " €" + "</td>");
                billBuilder.AppendLine("<td>" + productTotalWithoutTax + " €" + "</td>");
                billBuilder.AppendLine("</tr>");
                //Si tax, on ajoute au total du taux de la tva
                if (productBillEntries.Key.TaxEnum != Product.TAX.None)
                {
                    if (taxTotal.ContainsKey(productBillEntries.Key.Tax))
                        taxTotal[productBillEntries.Key.Tax] += productTotalWithoutTax;
                    else
                        taxTotal.Add(productBillEntries.Key.Tax, productTotalWithoutTax);
                }
                totalWithoutTax += productTotalWithoutTax;
            }
            billBuilder.AppendLine("<tr>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td>Total HT</td>");
            billBuilder.AppendLine("<td>" + totalWithoutTax + " €</td>");
            billBuilder.AppendLine("</tr>");
            bill.TaxAmount = 0;
            foreach (var tax in taxTotal)
            {
                decimal taxAmount = Math.Round(tax.Value / 100m * tax.Key, 2);
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td>TAV " + tax.Key + "%</td>");
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

            return billBuilder.ToString();
        }

        //CONSUMER BILL
        public static string GenerateHtmlBillContent(ConsumerBill bill, ApplicationDbContext dbContext)
        {
            StringBuilder builder = new StringBuilder();
            bill.OrderAmount = 0;

            //Entete de facture
            builder.AppendLine("<h2>Facture : " + bill.BillNumber + "</h2>");
            builder.AppendLine("<p>Numéro d'adhérent : " + bill.AdherentStolon.LocalId + "<p>");
            builder.AppendLine("<p>Nom : " + bill.Adherent.Name + "<p>");
            builder.AppendLine("<p>Prénom : " + bill.Adherent.Surname + "<p>");
            builder.AppendLine("<p>Téléphone : " + bill.Adherent.PhoneNumber + "<p>");
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
            foreach (var tmpBillEntry in bill.BillEntries)
            {
                var billEntry = dbContext.BillEntrys.Include(x => x.ProductStock).ThenInclude(x => x.Product).First(x => x.Id == tmpBillEntry.Id);
                decimal total = Convert.ToDecimal(billEntry.UnitPrice * billEntry.Quantity);
                builder.AppendLine("<tr>");
                builder.AppendLine("<td>" + billEntry.Name + "</td>");
                builder.AppendLine("<td>" + billEntry.UnitPrice + " €" + "</td>");
                builder.AppendLine("<td>" + billEntry.QuantityString + "</td>");
                builder.AppendLine("<td>" + total + " €" + "</td>");
                builder.AppendLine("</tr>");
                bill.OrderAmount += total;
            }
            builder.AppendLine("</table>");
            builder.AppendLine("<p>Montant total : " + bill.OrderAmount + " €</p>");
            return builder.ToString();
        }

        private static T CreateBill<T>(AdherentStolon userStolon, List<BillEntry> billEntries) where T : class, IBill, new()
        {
            IBill bill = new T();
            bill.BillEntries = billEntries;
            bill.BillNumber = GenerateBillNumber(userStolon.Stolon.ShortLabel, userStolon.LocalId, bill is ProducerBill);
            bill.AdherentStolon = userStolon;
            bill.State = BillState.Pending;
            bill.EditionDate = DateTime.Now;
            return bill as T;
        }

        private static string GenerateBillNumber(string shortLabel, int localId, bool isProducerBill)
        {
            //ShortStolonName_LocalId(P)_YearNumber_WeekNumber
            //Exemple : "Privas_12_2017_25
            string billNumber = shortLabel + "_" + localId;
            if (isProducerBill)
                billNumber += "P";
            billNumber += "_" + DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            return billNumber;
        }

        public static void GeneratePDF(IBill bill)
        {
            string billWebAddress = Path.Combine("http://", Configurations.SiteUrl, "Bills", "ShowBill", bill.BillNumber).Replace("\\", "/");
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
