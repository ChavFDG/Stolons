using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using MoreLinq;
using Stolons.Helpers;
using Stolons.Services;
using Stolons.Models;
using Stolons.Models.Users;
using static Stolons.Models.Product;
using Syncfusion.HtmlConverter;

namespace Stolons.Tools
{
    public static class BillGenerator
    {
        private static Dictionary<Guid, Stolon.Modes> lastModes = new Dictionary<Guid, Stolon.Modes>();

        private static ILogger<String> Logger;

        private static bool shouldRun = true;

        public static void ManageBills()
        {
            Logger = DotnetHelper.GetLogger<String>();
            GetCurrentStolonsModes();
            while (shouldRun)
            {
                using (var scope = DotnetHelper.GetNewScope())
                {
                    using (ApplicationDbContext dbContext = DotnetHelper.getDbContext(scope))
                    {
                        foreach (Stolon stolon in dbContext.Stolons.AsNoTracking().ToList())
                        {
                            //For new Stolon
                            if (!lastModes.Keys.Contains(stolon.Id))
                                lastModes.Add(stolon.Id, stolon.GetMode());

                            Stolon.Modes currentMode = stolon.GetMode();
                            if (lastModes[stolon.Id] == Stolon.Modes.Order && currentMode == Stolon.Modes.DeliveryAndStockUpdate)
                            {
#if (DEBUG)
                                DebugRemoveBills(dbContext);
#endif
                                TriggerDeliveryAndStockUpdateMode(stolon, dbContext);
                            }
                            else if (lastModes[stolon.Id] == Stolon.Modes.DeliveryAndStockUpdate && currentMode == Stolon.Modes.Order)
                            {
                                TriggerOrderMode(stolon, dbContext);
                            }
                            lastModes[stolon.Id] = currentMode;
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public static void StopThread()
        {
            shouldRun = false;
        }

        private static void GetCurrentStolonsModes()
        {
            using (var scope = DotnetHelper.GetNewScope())
            {
                using (ApplicationDbContext dbContext = DotnetHelper.getDbContext(scope))
                {
                    foreach (Stolon stolon in dbContext.Stolons.AsNoTracking().ToList())
                    {
                        if (!lastModes.Keys.Contains(stolon.Id))
                            lastModes.Add(stolon.Id, stolon.GetMode());
                    }
                }
            }
        }

        private static void TriggerOrderMode(Stolon stolon, ApplicationDbContext dbContext)
        {
            foreach (ProductStockStolon productStock in dbContext.ProductsStocks.Include(x => x.AdherentStolon).Include(x => x.Product).Where(x => x.AdherentStolon.StolonId == stolon.Id).ToList())
            {
                if (productStock.State == ProductState.Stock)
                {
                    productStock.State = ProductState.Disabled;
                }
            }
            dbContext.SaveChanges();
            //Send email to all adherent that have subscribe to product by mail
            string htmlMessage = "Bonjour, les commandes sont ouverte chez " + stolon.Label + ". Vous pouvez dès maintenant et jusqu'à " + stolon.DeliveryAndStockUpdateDayStartDate.ToFrench() + " " + stolon.DeliveryAndStockUpdateDayStartDateHourStartDate + ":" + stolon.DeliveryAndStockUpdateDayStartDateMinuteStartDate + " commander vos produits sur " + Configurations.Application.StolonsUrl;
            //TODO générer un jolie message avec la liste des produits
            foreach (var adherentStolon in dbContext.AdherentStolons.Include(x => x.Adherent).Where(x => x.StolonId == stolon.Id && x.Adherent.ReceivedProductListByEmail).AsNoTracking())
            {
                AuthMessageSender.SendEmail(stolon.Label,
                                adherentStolon.Adherent.Email,
                                adherentStolon.Adherent.Name + " " + adherentStolon.Adherent.Surname,
                                "Commande ouverte chez " + stolon.Label,
                                htmlMessage);
            }
        }

        private static void TriggerDeliveryAndStockUpdateMode(Stolon stolon, ApplicationDbContext dbContext)
        {
            //Consumer (create bills)
            List<ValidatedWeekBasket> consumerWeekBaskets = dbContext.ValidatedWeekBaskets
            .Include(x => x.BillEntries)
            .Include(x => x.AdherentStolon)
            .Include(x => x.AdherentStolon.Stolon)
            .Include(x => x.AdherentStolon.Adherent)
            .Where(x => x.AdherentStolon.StolonId == stolon.Id)
            .ToList();

            foreach (var weekBasket in consumerWeekBaskets)
            {
                //Remove TempWeekBasket and linked billEntry
                var tempWeekBasketToRemove = dbContext.TempsWeekBaskets.FirstOrDefault(x => x.AdherentStolonId == weekBasket.AdherentStolon.Id);
                if (tempWeekBasketToRemove == null)
                    continue;
                var linkedBillEntriesToRemove = dbContext.BillEntrys.Where(x => x.TempWeekBasketId == tempWeekBasketToRemove.Id).ToList();
                dbContext.RemoveRange(linkedBillEntriesToRemove);
                dbContext.SaveChanges();
                dbContext.Remove(tempWeekBasketToRemove);
                dbContext.SaveChanges();
                //Creates new bill entryes 
                List<BillEntry> billEntries = new List<BillEntry>();
                foreach (var oldBillEntry in weekBasket.BillEntries.ToList())
                {
                    BillEntry newBillEntry = oldBillEntry.Clone();
                    billEntries.Add(newBillEntry);
                    dbContext.Remove(oldBillEntry);
                    dbContext.Add(newBillEntry);
                    dbContext.SaveChanges();
                }
                dbContext.Remove(weekBasket);
                dbContext.SaveChanges();

                //Generate bill for consumer
                ConsumerBill consumerBill = CreateBill<ConsumerBill>(weekBasket.AdherentStolon, billEntries);
                consumerBill.HtmlBillContent = GenerateHtmlBillContent(consumerBill, dbContext);
                dbContext.Add(consumerBill);
                dbContext.SaveChanges();
            }

            List<ProducerBill> producerBills = new List<ProducerBill>();
            List<ConsumerBill> consumerBills = dbContext.ConsumerBills
            .Include(x => x.BillEntries)
            .ThenInclude(x => x.ProductStock)
            .ThenInclude(x => x.Product)
            .Include(x => x.AdherentStolon)
            .Include(x => x.AdherentStolon.Adherent)
            .Include(x => x.AdherentStolon.Stolon)
            .Where(x => x.AdherentStolon.Stolon.Id == stolon.Id && x.EditionDate.GetIso8601WeekOfYear() == DateTime.Now.GetIso8601WeekOfYear() && x.EditionDate.Year == DateTime.Now.Year)
            .ToList();

            //Producer (creates bills)
            foreach (var producer in dbContext.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).Where(x => x.StolonId == stolon.Id && x.IsProducer).ToList())
            {
                List<BillEntry> billEntries = new List<BillEntry>();
                foreach (var consumerBill in consumerBills)
                {
                    foreach (var billEntry in consumerBill.BillEntries.Where(billEntry => billEntry.ProductStock.Product.ProducerId == producer.AdherentId))
                    {
                        billEntries.Add(billEntry);
                    }
                }
                //Generate bill for producer
                ProducerBill bill = CreateBill<ProducerBill>(producer, billEntries);
                bill.HtmlBillContent = GenerateHtmlBillContent(bill, dbContext);
                bill.HtmlOrderContent = GenerateHtmlOrderContent(bill, dbContext);
                producerBills.Add(bill);
                if (billEntries.Any())
                {
                    dbContext.Add(bill);
                    dbContext.SaveChanges();
                }
            }

            //Stolons
            StolonsBill stolonsBill = GenerateBill(stolon, consumerBills, dbContext);
            stolonsBill.Producers = producerBills.Count;
            if (dbContext.StolonsBills.Any(x => x.BillNumber == stolonsBill.BillNumber))
            {
                StolonsBill tempBill = dbContext.StolonsBills.FirstOrDefault(x => x.BillNumber == stolonsBill.BillNumber);
                tempBill.Amount = stolonsBill.Amount;
                tempBill.EditionDate = stolonsBill.EditionDate;
                tempBill.HtmlBillContent = stolonsBill.HtmlBillContent;
                tempBill.ProducersFee = stolonsBill.ProducersFee;
            }
            else
                dbContext.Add(stolonsBill);

            dbContext.SaveChanges();
            //Move stolon's products to stock 
            foreach (ProductStockStolon productStock in dbContext.ProductsStocks.Include(x => x.AdherentStolon).Include(x => x.Product).Where(x => x.AdherentStolon.StolonId == stolon.Id).ToList())
            {
                if (productStock.State == ProductState.Enabled && productStock.Product.StockManagement == StockType.Week)
                {
                    productStock.State = ProductState.Stock;
                    productStock.RemainingStock = productStock.WeekStock;
                }
            }
            //

            dbContext.SaveChanges();
            //For stolons
            try
            {
                GeneratePDF(stolonsBill.HtmlBillContent, stolonsBill.GetStolonBillFilePath());
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
                Task.Factory.StartNew(() => { GenerateOrderPdfAndSendEmail(bill); });

            }

            //Bills (save bills and send mails to user)
            foreach (var bill in consumerBills)
            {
                Task.Factory.StartNew(() => { GenerateOrderPdfAndSendEmail(bill); });
            }
        }

        private static void GenerateOrderPdfAndSendEmail(ProducerBill bill)
        {
            try
            {
                if (bill.BillEntries.Count == 0)
                {
                    AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                bill.AdherentStolon.Adherent.Email,
                                bill.AdherentStolon.Adherent.CompanyName,
                                "Aucune commande chez " + bill.AdherentStolon.Stolon.Label + " cette semaine.",
                                "<h3>Aucune commande chez  " + bill.AdherentStolon.Stolon.Label + " cette semaine.");
                }
                else
                {
                    //Generate pdf file
                    bool hasFile = GenerateOrderPDF(bill);
                    //Send mail to producer
                    try
                    {
                        string message = bill.HtmlOrderContent;
                        if (hasFile)
                            message += "<h3>En pièce jointe votre bon de commande de la semaine chez " + bill.AdherentStolon.Stolon.Label + " (Bon de commande " + bill.BillNumber + ")</h3>";
                        AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                        bill.AdherentStolon.Adherent.Email,
                                        bill.AdherentStolon.Adherent.CompanyName,
                                        "Votre bon de commande de la semaine chez " + bill.AdherentStolon.Stolon.Label + " (Bon de commande " + bill.BillNumber + ")",
                                        message,
                                        hasFile ? File.ReadAllBytes(bill.GetOrderFilePath()) : null,
                                        "Bon de commande " + bill.GetOrderFileName());
                    }
                    catch (Exception exept)
                    {
                        Logger.LogError("Error on sending mail " + exept);
                    }
                }
            }
            catch (Exception exept)
            {
                AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                Configurations.Application.ContactMailAddress,
                                "Stolons",
                                "Erreur lors de la génération de la facture " + bill.BillNumber + " à " + bill.AdherentStolon.Adherent.Email,
                                "Message d'erreur : " + exept.Message);
            }
        }
        private static void GenerateOrderPdfAndSendEmail(ConsumerBill bill)
        {
            try
            {
                //Generate pdf file
                bool hasFile = GenerateBillPDF(bill);
                //Send mail to consumer
                string message = "<h3>" + bill.AdherentStolon.Stolon.OrderDeliveryMessage + "</h3>";
                message += "<br/>";
                message += "<h4>Votre comande de la semaine (Commande " + bill.BillNumber + ")" + ")</h4>";
                message += bill.HtmlBillContent;
                if (hasFile)
                    message += "<br/><h4>Retrouver aussi en pièce jointe votre commande de la semaine (Commande " + bill.BillNumber + ")</h4>";
                if (bill.AdherentStolon.Token > 0)
                    message += "<p>Vous avez " + bill.AdherentStolon.Token + "Ṩ, pensez à payer avec vos stols lors de la récupération de votre commande.</p>";

                try
                {
                    AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                bill.AdherentStolon.Adherent.Email,
                                bill.AdherentStolon.Adherent.Name,
                                "Votre commande de la semaine (Commande " + bill.BillNumber + ")",
                                message,
                                hasFile ? File.ReadAllBytes(bill.GetBillFilePath()) : null,
                                "Commande " + bill.GetBillFileName());
                }
                catch (Exception exept)
                {
                    Logger.LogError("Error on sending mail " + exept);
                }
            }
            catch (Exception exept)
            {
                AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                                Configurations.Application.ContactMailAddress,
                                "Stolons",
                                "Erreur lors de la génération de la commande " + bill.BillNumber + " à " + bill.AdherentStolon.Adherent.Email,
                                "Message d'erreur : " + exept.Message);
            }
        }

        private static string GetNumberSurnameName(this AdherentStolon adherentStolon)
        {
            return adherentStolon.LocalId + " / " + adherentStolon.Adherent.Surname + " / " + adherentStolon.Adherent.Name;
        }

        private static void AddBootstrap(this StringBuilder builder)
        {
            builder.Insert(0, "<meta charset=\"UTF-8\"><head><link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css\"></head><body>");
            builder.AppendLine("</body>");
        }

        private static void AddFooterAndHeaderRemoval(this StringBuilder builder)
        {
            builder.Insert(0, @"<style>@media print {
                                           @page { margin: 0; }
                                           body { margin: 1.6cm; }
                                         }</style>");
        }

        private static StolonsBill GenerateBill(Stolon stolon, List<ConsumerBill> consumersBills, ApplicationDbContext dbContext)
        {
            StringBuilder builder = new StringBuilder();
            string billNumber = stolon.ShortLabel + "_" + DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            StolonsBill bill = new StolonsBill(billNumber)
            {
                Stolon = stolon,
                Amount = 0,
                ProducersFee = stolon.ProducersFee,
                Consumers = consumersBills.Count
            };

            if (!consumersBills.Any())
            {
                builder.AppendLine("Rien cette semaine !");
            }
            else
            {
                builder.AppendLine("<h1>Commandes numéro : " + billNumber + "</h1>");
                builder.AppendLine("<p>Année : " + DateTime.Now.Year);
                builder.AppendLine("<br>Semaine : " + DateTime.Now.GetIso8601WeekOfYear());
                builder.AppendLine("<br><b>Nombre de panier : " + consumersBills.Count + "</b></p>");
                builder.AppendLine("<p>Adhérents ayant un panier : <small><br>");
                consumersBills.OrderBy(x => x.AdherentStolon.LocalId)
                    .ForEach(x => builder.AppendLine("<a href=\"#" + x.AdherentStolon.LocalId + "\">" + x.AdherentStolon.GetNumberSurnameName() + "</a><br>"));
                builder.AppendLine("</small></p>");

                builder.AppendLine("<div style=\"page-break-after:always\"></div>");
                int count = 0;
                foreach (var consumerBill in consumersBills.OrderBy(x => x.AdherentStolon.LocalId))
                {
                    count++;
                    builder.AppendLine("<h1 id=\"" + consumerBill.AdherentStolon.LocalId + "\">Adhérent : " + consumerBill.AdherentStolon.LocalId + " / " + consumerBill.AdherentStolon.Adherent.Surname + " / " + consumerBill.AdherentStolon.Adherent.Name + "</h1>");
                    builder.AppendLine("<p>Facture : " + consumerBill.BillNumber + "</p>");
                    builder.AppendLine("<p>Téléphone : " + consumerBill.AdherentStolon.Adherent.PhoneNumber + "</p>");
                    builder.AppendLine("<p>Total à régler : " + consumerBill.TotalPrice.ToString("0.00") + " €</p>");

                    //Create list of bill entry by product
                    var billEntryByProducer = consumerBill.BillEntries.GroupBy(x => x.ProducerBill);

                    builder.AppendLine("<h2>Commande par producteur</h2>");

                    builder.AppendLine("<table class=\"table\">");
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<th>Producteur</th>");
                    builder.AppendLine("<th>Produit</th>");
                    builder.AppendLine("<th>Quantité</th>");
                    builder.AppendLine("</tr>");

                    foreach (var producerBillsEntry in billEntryByProducer.OrderBy(x => x.Key.AdherentStolon.Adherent.CompanyName))
                    {
                        builder.AppendLine("<tr>");
                        builder.AppendLine("<td colspan=\"3\" style=\"border-top:1px solid;\">" + "<b>" + producerBillsEntry.Key.AdherentStolon.Adherent.CompanyName + "</b>" + "</td>");
                        builder.AppendLine("</tr>");
                        foreach (var billEntry in producerBillsEntry.OrderBy(x => x.ProductStock.Product.Name))
                        {
                            builder.AppendLine("<tr>");
                            builder.AppendLine("<td></td>");
                            builder.AppendLine("<td>" + billEntry.ProductStock.Product.Name + "</td>");
                            builder.AppendLine("<td>" + billEntry.QuantityString + "</td>");
                            builder.AppendLine("</tr>");

                        }
                    }
                    builder.AppendLine("</table>");
                    if (count != consumersBills.Count)
                        builder.AppendLine("<div style=\"page-break-after:always\"></div>");

                }


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
                  builder.AppendLine("<p>Total à régler : " + weekBasket.TotalPrice.ToString("0.00") + " €</p>");


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
            builder.AddBootstrap();
            builder.AddFooterAndHeaderRemoval();
            bill.HtmlBillContent = builder.ToString();
            //TODO envisager de créer les HtmlBillContent via des cshtml plutot que comme ca

            return bill;
        }

        //PRODUCER BILL

        private static string GenerateHtmlOrderContent(ProducerBill bill, ApplicationDbContext dbContext)
        {
            //GENERATION COMMANDE
            StringBuilder orderBuilder = new StringBuilder();
            //Entete de facture
            //  Producteur
            orderBuilder.AppendLine("<h2> Commande n°" + bill.OrderNumber + "</h2>");
            orderBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.CompanyName + "<br>");
            orderBuilder.AppendLine("<Année : " + DateTime.Now.Year + "<br>");
            orderBuilder.AppendLine("Semaine : " + DateTime.Now.GetIso8601WeekOfYear() + "<br></p>");
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
                orderBuilder.AppendLine("<td colspan=\"3\" style=\"border-top:1px solid;\">" + "<b>" + group.Key.LocalId + "</b>" + "</td>");
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

            orderBuilder.AppendLine("<h3>Facturation</h3>");
            orderBuilder.AppendLine("<p>" + "Montant total de la commande  " + bill.OrderAmount.ToString("0.00") + "€<br>");
            orderBuilder.AppendLine("Montant total du % du Stolons " + bill.FeeAmount.ToString("0.00") + "€<br>");
            orderBuilder.AppendLine("Montant total à percevoir     <b>" + bill.BillAmount.ToString("0.00") + "€</b> <i>(Dont " + bill.TaxAmount.ToString("0.00") + "€ TVA)</i></p>");
            orderBuilder.AddBootstrap();
            orderBuilder.AddFooterAndHeaderRemoval();
            return orderBuilder.ToString();
        }

        public static string GenerateHtmlBillContent(ProducerBill bill, ApplicationDbContext dbContext)
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
            billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.CompanyName + "<p>");
            billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.Surname?.ToUpper() + " " + bill.AdherentStolon.Adherent.Name?.ToUpper() + "<p>");
            billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.Address + "</p>");
            billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.PostCode + " " + bill.AdherentStolon.Adherent.City?.ToUpper() + "</p>");
            billBuilder.AppendLine("<br>");
            billBuilder.AppendLine("<p>Facture n° " + bill.BillNumber + "</p>");
            billBuilder.AppendLine("<p>Année : " + DateTime.Now.Year);
            billBuilder.AppendLine("<p>Semaine : " + DateTime.Now.GetIso8601WeekOfYear());

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
                billBuilder.AppendLine("<td>" + productTotalWithoutTax.ToString("0.00") + " €" + "</td>");
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
            billBuilder.AppendLine("<td>" + totalWithoutTax.ToString("0.00") + " €</td>");
            billBuilder.AppendLine("</tr>");
            bill.TaxAmount = 0;
            foreach (var tax in taxTotal)
            {
                decimal taxAmount = Math.Round(tax.Value / 100m * tax.Key, 2);
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td>TVA " + tax.Key + "%</td>");
                billBuilder.AppendLine("<td>" + taxAmount.ToString("0.00") + " €</td>");
                billBuilder.AppendLine("</tr>");
                bill.TaxAmount += taxAmount;
            }
            billBuilder.AppendLine("<tr>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td></td>");
            billBuilder.AppendLine("<td>Net à payer</td>");
            billBuilder.AppendLine("<td>" + bill.BillAmount.ToString("0.00") + " €</td>");
            billBuilder.AppendLine("</tr>");
            billBuilder.AppendLine("</table>");

            billBuilder.AddBootstrap();
            billBuilder.AddFooterAndHeaderRemoval();

            return billBuilder.ToString();
        }

        //CONSUMER BILL
        public static string GenerateHtmlBillContent(ConsumerBill bill, ApplicationDbContext dbContext)
        {
            StringBuilder builder = new StringBuilder();
            bill.OrderAmount = 0;

            //Entete de facture
            builder.AppendLine("<h2>Commande : " + bill.BillNumber + "</h2>");
            builder.AppendLine("<p>Numéro d'adhérent : " + bill.AdherentStolon.LocalId + "<br>");
            builder.AppendLine("Nom : " + bill.AdherentStolon.Adherent.Name + "<br>");
            builder.AppendLine("Prénom : " + bill.AdherentStolon.Adherent.Surname + "<br>");
            builder.AppendLine("Téléphone : " + bill.AdherentStolon.Adherent.PhoneNumber + "<br>");
            builder.AppendLine("Année : " + DateTime.Now.Year + "<br>");
            builder.AppendLine("Semaine : " + DateTime.Now.GetIso8601WeekOfYear() + "</p>");
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
                builder.AppendLine("<td>" + billEntry.UnitPrice.ToString("0.00") + " €" + "</td>");
                builder.AppendLine("<td>" + billEntry.QuantityString + "</td>");
                builder.AppendLine("<td>" + total.ToString("0.00") + " €" + "</td>");
                builder.AppendLine("</tr>");
                bill.OrderAmount += total;
            }
            builder.AppendLine("</table>");
            builder.AppendLine("<p>Montant total : " + bill.OrderAmount.ToString("0.00") + " €</p>");
            builder.AddBootstrap();
            builder.AddFooterAndHeaderRemoval();
            return builder.ToString();
        }

        private static T CreateBill<T>(AdherentStolon adherentStolon, List<BillEntry> billEntries) where T : class, IBill, new()
        {
            IBill bill = new T();
            bill.BillId = Guid.NewGuid();
            bill.BillEntries = billEntries;
            bill.BillNumber = GenerateBillNumber(adherentStolon.Stolon.ShortLabel, adherentStolon.LocalId, bill is ProducerBill);
            if (bill is ProducerBill)
                (bill as ProducerBill).OrderNumber = GenerateOrderNumber(adherentStolon.Stolon.ShortLabel, adherentStolon.LocalId);
            bill.AdherentStolon = adherentStolon;
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

        private static string GenerateOrderNumber(string shortLabel, int localId)
        {
            //ShortStolonName_LocalId(P)_YearNumber_WeekNumber
            //Exemple : "Privas_12_2017_25
            string orderNumber = shortLabel + "_" + localId;
            orderNumber += "BC";
            orderNumber += "_" + DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            return orderNumber;
        }

        public static bool GenerateOrderPDF(ProducerBill bill)
        {
            return GeneratePDF(bill.HtmlOrderContent, bill.GetOrderFilePath());
        }

        public static bool GenerateBillPDF(IBill bill)
        {
            return GeneratePDF(bill.HtmlBillContent, bill.GetBillFilePath());
        }

        public static bool GeneratePDF(string htmlContent, string fullPath)
        {
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();

            WebKitConverterSettings settings = new WebKitConverterSettings();
            settings.Margin.All = 8;

            //Set WebKit path
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                settings.WebKitPath = Path.Combine(Configurations.Environment.ContentRootPath, "lib", "QtBinariesDotNetCore");
            else
                settings.WebKitPath = Path.Combine(Configurations.Environment.ContentRootPath, "lib", "QtBinaries");

            //Assign WebKit settings to HTML converter
            htmlConverter.ConverterSettings = settings;
                        
            //Convert URL to PDF
            Syncfusion.Pdf.PdfDocument document = htmlConverter.Convert(htmlContent, "");
            
            //Save and close the PDF document 
            new FileInfo(fullPath).Directory.Create();
            using (var streamWriter = File.Create(fullPath))
            {
                document.Save(streamWriter);
            }
            document.Close(true);

            return true;
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

        //For tests and developpment, remove existing consumer bill and producer bill => That will never exist in normal mode cause they can only have one bill by week per user
        private static void DebugRemoveBills(ApplicationDbContext dbContext)
        {
            string billToRemove = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            foreach (var bill in dbContext.ConsumerBills.Where(x => x.BillNumber.EndsWith(billToRemove)).Include(x => x.BillEntries).ToList())
            {
                foreach (var billEntry in bill.BillEntries)
                {
                    billEntry.ConsumerBill = null;
                }
                dbContext.Remove(bill);
            }
            foreach (var bill in dbContext.ProducerBills.Where(x => x.BillNumber.EndsWith(billToRemove)).Include(x => x.BillEntries).ToList())
            {
                foreach (var billEntry in bill.BillEntries)
                {
                    billEntry.ProducerBill = null;
                }
                dbContext.Remove(bill);
            }
            dbContext.SaveChanges();
            var billEntriesToRemove = dbContext.BillEntrys.Where(x => x.ConsumerBillId == null && x.ProducerBillId == null && x.TempWeekBasketId == null && x.ValidatedWeekBasketId == null).ToList();
            dbContext.RemoveRange(billEntriesToRemove);
            dbContext.SaveChanges();
        }

    }
}
