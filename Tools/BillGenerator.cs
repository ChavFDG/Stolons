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
using PhantomJs.NetCore;

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
                                DebugRemoveBills(dbContext, stolon);
#endif
                                if(stolon.IsModeSimulated)
                                {
                                    DebugRemoveBills(dbContext, stolon);
                                }
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

        public static Timer orderRememberTimer;

        public static void StartOrderRemember()
        {
            orderRememberTimer = new Timer((e) =>
            {
                TriggerOrderRemember();

            }, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        private static void TriggerOrderRemember()
        {
            using (var scope = DotnetHelper.GetNewScope())
            {
                using (ApplicationDbContext dbContext = DotnetHelper.getDbContext(scope))
                {
                    foreach (Stolon stolon in dbContext.Stolons.Where(x => x.BasketPickUpStartDay == DateTime.Now.DayOfWeek && x.OrderRememberHour == DateTime.Now.Hour).ToList())
                    {
                        var consumerBills = dbContext.ConsumerBills
                                            .Include(x => x.AdherentStolon)
                                            .Include(x => x.AdherentStolon.Adherent)
                                            .Include(x => x.AdherentStolon.Stolon)
                                            .Where(x => x.State == BillState.Pending && x.AdherentStolon.StolonId == stolon.Id && x.AdherentStolon.Adherent.ReceivedOrderRemember && !x.AdherentStolon.Deleted)
                                            .AsNoTracking()
                                            .ToList();
                        string message = "<h2>Rappel " + stolon.Label + " : panier à récupérer aujourd'hui</h2>";
                        message += "<b>"+stolon.OrderDeliveryMessage+"</b>";
                        message += "<br><i>En commandant en ligne sur notre site, vous vous engagez à venir chercher votre panier.<br>En cas d'indisponibilité, veuillez nous prévenir au plus tôt";
                        if (!String.IsNullOrWhiteSpace(stolon.PhoneNumber))
                            message += "au " + stolon.PhoneNumber;
                        message += ".</ i > ";

                        foreach (var consumerBill in consumerBills)
                        {
                            try
                            {
                                AuthMessageSender.SendEmail(consumerBill.AdherentStolon.Stolon.Label,
                                            consumerBill.AdherentStolon.Adherent.Email,
                                            consumerBill.AdherentStolon.Adherent.Name,
                                            "Rappel " + stolon.Label + " : panier à récupérer aujourd'hui ",
                                            message,
                                            null,
                                            "");
                            }
                            catch (Exception exept)
                            {
                                Logger.LogError("Error on sending mail " + exept);
                            }

                        }
                    }
                }
            }
        }

        public static void StopThread()
        {
            shouldRun = false;
            orderRememberTimer.Dispose();
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
            string htmlMessage = "Bonjour, les commandes sont ouvertes chez " + stolon.Label + ". Vous pouvez dès maintenant et jusqu'à " + stolon.DeliveryAndStockUpdateDayStartDate.ToFrench() + " " + stolon.DeliveryAndStockUpdateDayStartDateHourStartDate + ":" + stolon.DeliveryAndStockUpdateDayStartDateMinuteStartDate + " commander vos produits sur " + Configurations.Application.StolonsUrl;
            //TODO générer un jolie message avec la liste des produits
            foreach (var adherentStolon in dbContext.AdherentStolons.Include(x => x.Adherent).Where(x => x.StolonId == stolon.Id && x.Adherent.ReceivedProductListByEmail && !x.Deleted).AsNoTracking())
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

            //Remove Temps week basket from consumer to be sure that the next week the temp week basket will not stay
            List<TempWeekBasket> consumerTempsWeekBaskets = dbContext.TempsWeekBaskets
            .Include(x => x.BillEntries)
            .Include(x => x.AdherentStolon)
            .Include(x => x.AdherentStolon.Stolon)
            .Include(x => x.AdherentStolon.Adherent)
            .Where(x => x.AdherentStolon.StolonId == stolon.Id)
            .ToList();
            foreach(var tempWeekBasket in consumerTempsWeekBaskets)
            {
                var linkedBillEntriesToRemove = dbContext.BillEntrys.Where(x => x.TempWeekBasketId == tempWeekBasket.Id).ToList();
                dbContext.RemoveRange(linkedBillEntriesToRemove);
                dbContext.SaveChanges();
            }
            dbContext.TempsWeekBaskets.RemoveRange(consumerTempsWeekBaskets);
            dbContext.SaveChanges();
            //

            List<ProducerBill> producerBills = new List<ProducerBill>();
            List<ConsumerBill> consumerBills = dbContext.ConsumerBills
            .Include(x => x.BillEntries)
            .ThenInclude(x => x.ProductStock)
            .ThenInclude(x => x.Product)
            .Include(x => x.BillEntries)
            .ThenInclude(x => x.ConsumerBill)
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

            GenerateStolonBill(stolon, dbContext, consumerBills);


            // => Producer, send mails
            foreach (var bill in producerBills)
            {
                Task.Factory.StartNew(() => { GenerateOrderPdfAndSendEmail(bill); });

            }

            // (save bills and send mails to user)
            foreach (var bill in consumerBills)
            {
                Task.Factory.StartNew(() => { GenerateOrderPdfAndSendEmail(bill); });
            }

        }

        public static StolonsBill GenerateStolonBill(Stolon stolon, ApplicationDbContext dbContext, List<ConsumerBill> consumerBills)
        {
            try
            {
                StolonsBill stolonBill = GenerateBill(stolon, consumerBills, dbContext);
                dbContext.SaveChanges();
                if (stolonBill.GenerationError)
                {
                    var weekStolonBill = dbContext.StolonsBills.Include(x => x.BillEntries).ThenInclude(x => x.ProducerBill).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
                                                                .Include(x => x.BillEntries).ThenInclude(x => x.ConsumerBill).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
                                                                .Include(x => x.BillEntries).ThenInclude(x => x.ProductStock).ThenInclude(x => x.Product)
                                                                .First(x => x.BillNumber == stolonBill.BillNumber);
                    weekStolonBill.HtmlBillContent = GenerateHtmlContent(weekStolonBill);
                }
                var allBillEntries = new List<BillEntry>();
                consumerBills.ForEach(bill => bill.BillEntries.ForEach(billEntry => allBillEntries.Add(billEntry)));
                stolonBill.BillEntries = new List<BillEntry>(allBillEntries);
                stolonBill.UpdateBillInfo();
                if (dbContext.StolonsBills.Any(x => x.BillNumber == stolonBill.BillNumber))
                {
                    dbContext.Remove(dbContext.StolonsBills.FirstOrDefault(x => x.BillNumber == stolonBill.BillNumber));
                    dbContext.SaveChanges();
                }
                dbContext.Add(stolonBill);
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
                dbContext.SaveChanges();
                //Pdf
                try
                {
                    GeneratePDF(stolonBill.HtmlBillContent, stolonBill.GetStolonBillFilePath());
                }
                catch (Exception exept)
                {
                    AuthMessageSender.SendEmail(stolon.Label,
                                    Configurations.Application.ContactMailAddress,
                                    "Stolons",
                                    "Erreur lors de la génération de la facture Stolons en pdf",
                                    "Message d'erreur : " + exept.Message);
                }
                return stolonBill;
            }
            catch (Exception exept)
            {
                AuthMessageSender.SendEmail(stolon.Label,
                                Configurations.Application.ContactMailAddress,
                                "Stolons",
                                "Erreur lors de la création puis génération de la facture Stolons",
                                "Message d'erreur : " + exept.Message);
            }
            return null;
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
                        {
                            message += "<h3>En pièce jointe votre bon de commande de la semaine chez " + bill.AdherentStolon.Stolon.Label + " (Bon de commande " + bill.BillNumber + ")</h3>";
                            if (bill.BillEntries.Any(x => x.IsNotAssignedVariableWeigh))
                            {
                                string productWeightLink = "http://" + Configurations.SiteUrl + @"\ProductsManagement";
                                message += "<h3><a href=\"" + productWeightLink + "\">/!\\ Vous avez des produits en attente de saisie de poids /!\\</a></h3>";
                            }

                        }
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
                message += "<h4>Votre commande de la semaine (Commande " + bill.BillNumber + ")" + ")</h4>";
                message += bill.HtmlBillContent;
                if (hasFile)
                    message += "<br/><h4>Retrouver aussi en pièce jointe votre commande de la semaine (Commande " + bill.BillNumber + ")</h4>";
                if (bill.AdherentStolon.Token > 0)
                    message += "<p>Vous avez " + bill.AdherentStolon.Token + "Ṩ, pensez à payer avec vos stols lors de la récupération de votre commande.</p>";
                message += "<p><i>Pour annuler votre commande, merci de nous prévenir au moins 24h à l'avance : "+bill.AdherentStolon.Stolon.ContactMailAddress+" " + bill.AdherentStolon.Stolon.PhoneNumber+" </i></p>";

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
            builder.Insert(0, "<html><meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><meta charset=\"UTF-8\"><head><link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css\"></head><body>");
            builder.AppendLine("</body><html>");
        }

        private static void AddFooterAndHeaderRemoval(this StringBuilder builder)
        {
            builder.Insert(0, @"<style>@media print {
                                           @page { margin: 0; }
                                           body { margin: 1.6cm; font-size: 12px; }
                                         }</style>");
        }

        private static StolonsBill GenerateBill(Stolon stolon, List<ConsumerBill> consumersBills, ApplicationDbContext dbContext)
        {
            string billNumber = stolon.ShortLabel + "_" + DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            StolonsBill bill = new StolonsBill(billNumber)
            {
                Stolon = stolon,
                Amount = 0,
                Consumers = consumersBills.Count
            };

            bill.HtmlBillContent = GenerateHtmlContent(bill, consumersBills);

            return bill;
        }

        public static string GenerateHtmlContent(StolonsBill bill)
        {
            List<ConsumerBill> consumersBills = bill.BillEntries.DistinctBy(x => x.ConsumerBillId).Select(x => x.ConsumerBill).ToList();
            return GenerateHtmlContent(bill, consumersBills);
        }

        public static string GenerateHtmlContent(StolonsBill bill, List<ConsumerBill> consumersBills)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                if (!consumersBills.Any())
                {
                    builder.AppendLine("Rien cette semaine !");
                }
                else
                {
                    builder.AppendLine("<h1>Commandes numéro : " + bill.BillNumber + "</h1>");
                    builder.AppendLine("<p>Année : " + DateTime.Now.Year);
                    builder.AppendLine("<br>Semaine : " + DateTime.Now.GetIso8601WeekOfYear());
                    builder.AppendLine("<br><b>Nombre de panier : " + consumersBills.Count + "</b></p>");
                    builder.AppendLine("<p>Adhérents ayant un panier : <small><br>");
                    consumersBills.Where(x => x.State != BillState.Cancelled).OrderBy(x => x.AdherentStolon.LocalId)
                        .ForEach(x => builder.AppendLine("<a href=\"#" + x.AdherentStolon.LocalId + "\">" + x.AdherentStolon.GetNumberSurnameName() + "</a><br>"));
                    builder.AppendLine("</small></p>");
                    if (bill.HasBeenModified)
                    {
                        builder.AppendLine("<div style=\"color:red\">");
                        builder.AppendLine("<p>Cette commande a été modifié pour les raisons suivante : </p>");
                        builder.AppendLine("<p>" + bill.ModificationReason + "</p>");
                        builder.AppendLine("</div>");
                    }

                    builder.AppendLine("<div style=\"page-break-after:always\"></div>");
                    int count = 0;
                    foreach (var consumerBill in consumersBills.Where(x => x.State != BillState.Cancelled).OrderBy(x => x.AdherentStolon.LocalId))
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
                                builder.AppendLine("<td>" + billEntry.QuantityString + (billEntry.IsNotAssignedVariableWeigh ? " (poids variable)" : "") + "</td>");
                                builder.AppendLine("</tr>");

                            }
                        }
                        builder.AppendLine("</table>");
                        if (count != consumersBills.Count)
                            builder.AppendLine("<div style=\"page-break-after:always\"></div>");

                    }
                }
                builder.AddBootstrap();
                builder.AddFooterAndHeaderRemoval();
                bill.GenerationError = false;
                return builder.ToString();

            }
            catch (Exception e)
            {
                bill.GenerationError = true;
                Logger.LogError(e, "Erreur lors de la génération de la stolon bill : " + e.Message);
                return "Erreur lors de la génération" + e.Message;
            }
        }

        //PRODUCER BILL

        public static string GenerateHtmlOrderContent(ProducerBill bill, ApplicationDbContext dbContext)
        {
            try
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
                orderBuilder.AppendLine("<th>Consommateur</th>");
                orderBuilder.AppendLine("<th>Quantité</th>");
                orderBuilder.AppendLine("</tr>");

                foreach (var productBillEntries in bill.BillEntries.Where(x => x.ConsumerBill.State != BillState.Cancelled).GroupBy(x => x.ProductStock.Product, x => x).OrderBy(x => x.Key.Name))
                {
                    int quantity = 0;
                    productBillEntries.ForEach(x => quantity += x.Quantity);
                    if (productBillEntries.First().IsAssignedVariableWeigh)
                        quantity = quantity / productBillEntries.Key.QuantityStep;
                    orderBuilder.AppendLine("<tr>");
                    orderBuilder.AppendLine("<td colspan=\"2\"><b>" + productBillEntries.Key.Name + "</b></td>");
                    orderBuilder.AppendLine("<td><b>" + productBillEntries.Key.GetQuantityString(quantity) + "</b></td>");
                    orderBuilder.AppendLine("</tr>");
                    foreach(var billEntry in productBillEntries.OrderBy(x=>x.ConsumerBill.AdherentStolon.LocalId))
                    {
                        orderBuilder.AppendLine("<tr>");
                        orderBuilder.AppendLine("<td></td>");
                        orderBuilder.AppendLine("<td>"+billEntry.ConsumerBill.AdherentStolon.LocalId+"</td>");
                        orderBuilder.AppendLine("<td>" + billEntry.GetQuantityString(billEntry.Quantity) + "</td>");
                        orderBuilder.AppendLine("</tr>");

                    }
                }
                orderBuilder.AppendLine("</table>");

                #endregion Par produit

                #region Par client
                orderBuilder.AppendLine("<h3>Commande par client</h3>");

                var billEntriesByConsumer = bill.BillEntries.Where(x => x.ConsumerBill.State != BillState.Cancelled).GroupBy(x => x.ConsumerBill.AdherentStolon);
                orderBuilder.AppendLine("<table class=\"table\">");
                orderBuilder.AppendLine("<tr>");
                orderBuilder.AppendLine("<th>Client</th>");
                orderBuilder.AppendLine("<th>Produit</th>");
                orderBuilder.AppendLine("<th>Quantité</th>");
                orderBuilder.AppendLine("<th></th>");
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
                        orderBuilder.AppendLine("<td>" + entries.GetQuantityString(entries.Quantity) + "</td>");
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
            catch (Exception e)
            {
                Logger.LogError(e, "Erreur lors de la génération de la producer order : " + e.Message);
                return "Erreur lors de la génération" + e.Message;
            }
        }

        public static string GenerateHtmlBillContent(ProducerBill bill, ApplicationDbContext dbContext)
        {
            try
            {
                //Calcul total amount
                decimal totalAmount = 0;
                foreach (var billEntry in bill.BillEntries.Where(x => x.ConsumerBill.State != BillState.Cancelled))
                {
                    totalAmount += billEntry.Price;
                }
                bill.OrderAmount = totalAmount;
                bill.ProducerFee = bill.AdherentStolon.ProducerFee;

                //GENERATION FACTURE
                StringBuilder billBuilder = new StringBuilder();
                //Entete de facture
                ////Producteur
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.CompanyName + "<p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.Surname?.ToUpper() + " " + bill.AdherentStolon.Adherent.Name?.ToUpper() + "<p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.Address + "</p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.PostCode + " " + bill.AdherentStolon.Adherent.City?.ToUpper() + "</p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.PhoneNumber + "</p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Adherent.Email + "</p>");
                billBuilder.AppendLine("<br>");
                ////Stolon
                billBuilder.AppendLine("<div style=\"float:right\">");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Stolon.Label + "<p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Stolon.Address + "<p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Stolon.PhoneNumber + "<p>");
                billBuilder.AppendLine("<p>" + bill.AdherentStolon.Stolon.ContactMailAddress + "<p>");
                billBuilder.AppendLine("</div>");
                ////Facture info
                billBuilder.AppendLine("<div style=\"font-weight:bold\">");
                billBuilder.AppendLine("Facture n° " + bill.BillNumber);
                billBuilder.AppendLine("Année : " + DateTime.Now.Year + ", semaine : " + DateTime.Now.GetIso8601WeekOfYear());
                billBuilder.AppendLine("</div>");
                //Tableau
                billBuilder.AppendLine("<br>");
                billBuilder.AppendLine("<table class=\"table\">");
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<th>Produit</th>");
                billBuilder.AppendLine("<th>Quantité</th>");
                billBuilder.AppendLine("<th>TVA</th>");
                billBuilder.AppendLine("<th>PU HT</th>");
                billBuilder.AppendLine("<th>TOTAL HT</th>");
                billBuilder.AppendLine("</tr>");
                //Taux tax / TotalTaxOlder => Total HT / Total Tax
                Dictionary<decimal, TotalTaxOlder> taxTotal = new Dictionary<decimal, TotalTaxOlder>();
                decimal totalWithoutTax = 0;
                foreach (var productBillEntries in bill.BillEntries.Where(x => x.ConsumerBill.State != BillState.Cancelled).GroupBy(x => x.ProductStock.Product, x => x).OrderBy(x => x.Key.Name))
                {
                    int quantity = 0;
                    productBillEntries.ForEach(x => quantity += x.Quantity);
                    decimal productTotalWithoutTax = Convert.ToDecimal(productBillEntries.First().UnitPriceWithoutTax * quantity);
                    decimal productTotalTax = Convert.ToDecimal(productBillEntries.First().UnitTax * quantity);
                    billBuilder.AppendLine("<tr>");
                    billBuilder.AppendLine("<td>" + productBillEntries.Key.Name + "</td>");
                    billBuilder.AppendLine("<td>" + (productBillEntries.Key.Type == SellType.VariableWeigh ? productBillEntries.Key.FormatQuantityString(quantity) : productBillEntries.Key.GetQuantityString(quantity)) + "</td>");
                    billBuilder.AppendLine("<td>" + (productBillEntries.Key.TaxEnum == Product.TAX.None ? "NA" : productBillEntries.Key.Tax.ToString("0.00") + " %</td>"));
                    billBuilder.AppendLine("<td>" + (productBillEntries.Key.Type == SellType.Piece ? productBillEntries.First().UnitPriceWithoutTax : productBillEntries.First().PriceWithoutTax).ToString("0.00") + " €" + "</td>");
                    billBuilder.AppendLine("<td>" + productTotalWithoutTax.ToString("0.00") + " €" + "</td>");
                    billBuilder.AppendLine("</tr>");
                    //Si tax, on ajoute au total du taux de la tva
                    if (productBillEntries.Key.TaxEnum != Product.TAX.None)
                    {
                        if (taxTotal.ContainsKey(productBillEntries.Key.Tax))
                        {
                            taxTotal[productBillEntries.Key.Tax].TotalPriceWithoutTaxe += productTotalWithoutTax;
                            taxTotal[productBillEntries.Key.Tax].TotalTax += productTotalTax;

                        }
                        else
                            taxTotal.Add(productBillEntries.Key.Tax, new TotalTaxOlder(productTotalWithoutTax, productTotalTax));
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
                    decimal taxAmount = Math.Round(tax.Value.TotalTax, 2);
                    billBuilder.AppendLine("<tr>");
                    billBuilder.AppendLine("<td></td>");
                    billBuilder.AppendLine("<td></td>");
                    billBuilder.AppendLine("<td></td>");
                    billBuilder.AppendLine("<td font-style=italic>TVA " + tax.Key.ToString("0.00") + "%</td>");
                    billBuilder.AppendLine("<td font-style=italic>" + taxAmount.ToString("0.00") + " €</td>");
                    billBuilder.AppendLine("</tr>");
                    bill.TaxAmount += taxAmount;
                }
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td>Total TVA</td>");
                billBuilder.AppendLine("<td>" + bill.TaxAmount.ToString("0.00") + " €</td>");
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td>Total TTC</td>");
                billBuilder.AppendLine("<td>" + bill.OrderAmount.ToString("0.00") + " €</td>");
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td font-style=italic>Commission (" + bill.ProducerFee + "%)</td>");
                billBuilder.AppendLine("<td font-style=italic>" + bill.FeeAmount.ToString("0.00") + " €</td>");
                billBuilder.AppendLine("<tr>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td></td>");
                billBuilder.AppendLine("<td font-style=bold>Net à payer</td>");
                billBuilder.AppendLine("<td font-style=bold>" + bill.BillAmount.ToString("0.00") + " €</td>");
                billBuilder.AppendLine("</tr>");
                billBuilder.AppendLine("</table>");

                billBuilder.AddBootstrap();
                billBuilder.AddFooterAndHeaderRemoval();

                return billBuilder.ToString();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Erreur lors de la génération de la producer bill : " + e.Message);
                return "Erreur lors de la génération" + e.Message;
            }
        }

        //CONSUMER BILL
        public static string GenerateHtmlBillContent(ConsumerBill bill, ApplicationDbContext dbContext)
        {
            try
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
                builder.AppendLine("<th>Prix unitaire ou au kg</th>");
                builder.AppendLine("<th>Quantité</th>");
                builder.AppendLine("<th>Prix total</th>");
                builder.AppendLine("</tr>");
                //
                bool notAssignedVariableWeigh = false;
                foreach (var tmpBillEntry in bill.BillEntries)
                {
                    var billEntry = dbContext.BillEntrys.Include(x => x.ProductStock).ThenInclude(x => x.Product).First(x => x.Id == tmpBillEntry.Id);
                    decimal total = 0;
                    if (billEntry.IsNotAssignedVariableWeigh)
                    {
                        total = Convert.ToDecimal(billEntry.ProductStock.Product.AveragePrice * billEntry.Quantity);
                        notAssignedVariableWeigh = true;
                    }
                    else
                    {
                        total = Convert.ToDecimal(billEntry.UnitPrice * billEntry.Quantity);
                    }
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<td>" + billEntry.Name + "</td>");
                    if (billEntry.Type == Product.SellType.Piece || billEntry.IsNotAssignedVariableWeigh)
                        builder.AppendLine("<td>" + billEntry.UnitPrice.ToString("0.00") + " €" + "</td>");
                    else
                        builder.AppendLine("<td>" + billEntry.WeightPrice.ToString("0.00") + " € / kg" + "</td>");
                    builder.AppendLine("<td>" + billEntry.QuantityString + "</td>");
                    builder.AppendLine("<td>" + total.ToString("0.00") + " €" + (billEntry.IsNotAssignedVariableWeigh ? " (poids variable)" : "") + "</td>");
                    builder.AppendLine("</tr>");
                    bill.OrderAmount += total;
                }
                builder.AppendLine("</table>");
                if(notAssignedVariableWeigh)
                    builder.AppendLine("<h2><b>Attention / information :</b></h2><p>Le montant total affiché sur votre facture ne prend pas en compte les poids variables non assignés. Lors de l'assignation des poids variables, vous recevrez un mail avec votre facture mise à jour.</p>");
                if (bill.State == BillState.Cancelled)
                    builder.AppendLine("<h2><b>Commande annulé</b></h2><p>" + bill.ModificationReason + "</p>");
                else
                    builder.AppendLine("<p>Montant total : " + bill.OrderAmount.ToString("0.00") + " €</p>");
                builder.AddBootstrap();
                builder.AddFooterAndHeaderRemoval();
                string test = builder.ToString();
                return builder.ToString();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Erreur lors de la génération de la consumer bill : " + e.Message);
                return "Erreur lors de la génération" + e.Message;
            }
        }

        private static T CreateBill<T>(AdherentStolon adherentStolon, List<BillEntry> billEntries) where T : class, IBill, new()
        {
            IBill bill = new T();
            bill.BillId = Guid.NewGuid();
            bill.BillEntries = billEntries;
            bill.BillNumber = GenerateBillNumber(adherentStolon.Stolon.ShortLabel, adherentStolon.LocalId, bill is ProducerBill);
            if (bill is ProducerBill)
            {
                (bill as ProducerBill).OrderNumber = GenerateOrderNumber(adherentStolon.Stolon.ShortLabel, adherentStolon.LocalId);
                billEntries.ForEach(x => x.ProducerBill = bill as ProducerBill);
            }
            else if (bill is ConsumerBill)
                billEntries.ForEach(x => x.ConsumerBill = bill as ConsumerBill);
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

        public static bool GeneratePDF(string htmlContent, string pdfFilePath)
        {
            // DotnetHelper.GetLogger<string>().LogError("Génération du PDF : " + pdfFilePath);

            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string phantomJsRootFolder = Path.Combine(currentDirectory, "PhantomJSRoot");

                // the pdf generator needs to know the path to the folder with .exe files.
                PdfGenerator generator = new PdfGenerator(phantomJsRootFolder);

                // Generate pdf from html and place in the current folder.
                string pathOftheGeneratedPdf = generator.GeneratePdf(htmlContent, pdfFilePath);

                return true;
            }
            catch (Exception except)
            {
                DotnetHelper.GetLogger<string>().LogError("Erreur lors de la génération du PDF : " + except.Message, except);
                Console.WriteLine("Erreur lors de la génération du PDF : " + except);
                return false;
            }

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
        private static void DebugRemoveBills(ApplicationDbContext dbContext, Stolon stolon)
        {
            string billToRemove = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            foreach (var bill in dbContext.ConsumerBills.Include(x => x.AdherentStolon).Where(x => x.BillNumber.EndsWith(billToRemove) && x.AdherentStolon.StolonId == stolon.Id).Include(x => x.BillEntries).ToList())
            {
                foreach (var billEntry in bill.BillEntries)
                {
                    billEntry.ConsumerBill = null;
                }
                dbContext.Remove(bill);
            }
            foreach (var bill in dbContext.ProducerBills.Where(x => x.BillNumber.EndsWith(billToRemove) && x.AdherentStolon.StolonId == stolon.Id).Include(x => x.BillEntries).ToList())
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

        private class TotalTaxOlder
        {

            public TotalTaxOlder(decimal productTotalWithoutTax, decimal price)
            {
                TotalPriceWithoutTaxe = productTotalWithoutTax;
                TotalTax = price;
            }

            public decimal TotalPriceWithoutTaxe { get; set; } = 0;
            public decimal TotalTax { get; set; } = 0;

        }

    }
}
