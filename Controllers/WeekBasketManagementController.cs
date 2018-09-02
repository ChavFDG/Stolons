using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Stolons.ViewModels.WeekBasketManagement;
using System;
using Stolons.Tools;
using Stolons.Helpers;
using static Stolons.Models.Transactions.Transaction;
using Stolons.Models.Transactions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Stolons.Services;
using MoreLinq;
using Microsoft.AspNetCore.Authorization;

namespace Stolons.Controllers
{
    public class WeekBasketManagementController : BaseController
    {

        public WeekBasketManagementController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        // GET: 
        [Authorize()]
        public IActionResult Index()
        {
            return View();
        }

        // GET: 
        public IActionResult History()
        {
            Stolon stolon = GetCurrentStolon();

            var adherentStolon = GetActiveAdherentStolon();

            var stolonsBills = _context.StolonsBills
                                .Include(x => x.Stolon)
                                .Include(x => x.BillEntries)
                                .Where(x => x.StolonId == stolon.Id)
                                .ToList();

            var consumersBills = _context.ConsumerBills
                                .Include(x => x.AdherentStolon)
                                .Include(x => x.AdherentStolon.Adherent)
                                .Include(x => x.AdherentStolon.Stolon)
                                .Where(x => x.AdherentStolon.StolonId == stolon.Id)
                                .OrderBy(x => x.AdherentStolon.Adherent.Id)
                                .AsNoTracking()
                                .ToList();

            var producerBills = _context.ProducerBills
                                .Include(x => x.AdherentStolon)
                                .Include(x => x.AdherentStolon.Adherent)
                                .Include(x => x.AdherentStolon.Stolon)
                                .Where(x => x.AdherentStolon.StolonId == stolon.Id)
                                .OrderBy(x => x.AdherentStolon.Adherent.Id)
                                .AsNoTracking()
                                .ToList();

            VmWeekBasketHistory vm = new VmWeekBasketHistory(adherentStolon, stolonsBills, consumersBills, producerBills);
            vm.Stolon = GetCurrentStolon();
            return View(vm);
        }

        public IActionResult LinkBillEntry()
        {
            var stolonsBills = _context.StolonsBills
                                .Include(x => x.Stolon)
                                .Include(x => x.BillEntries)
                                .ToList();

            var billEntries = _context.BillEntrys
                                .Include(x => x.ConsumerBill)
                                .Include(x => x.ProducerBill).ThenInclude(x => x.AdherentStolon)
                                .ToList();
            foreach (var stolonBill in stolonsBills.Where(x => x.BillEntries?.Any() != true))
            {
                stolonBill.BillEntries = new List<BillEntry>();

                foreach (var billEntry in billEntries.Where(x => x.ProducerBill != null).Where(x => x.ProducerBill.EditionDate.Year == stolonBill.EditionDate.Year && x.ProducerBill.EditionDate.GetIso8601WeekOfYear() == stolonBill.EditionDate.GetIso8601WeekOfYear() && stolonBill.StolonId == x.ProducerBill.AdherentStolon.StolonId))
                {
                    stolonBill.BillEntries.Add(billEntry);
                }
                stolonBill.UpdateBillInfo();
                _context.SaveChanges();
            }
            _context.SaveChanges();



            return Redirect(nameof(History));
        }


        // GET: 
        public IActionResult WeekBaskets()
        {
            Stolon stolon = GetCurrentStolon();
            var adherentStolon = GetActiveAdherentStolon();
            VmWeekBasketManagement vm = new VmWeekBasketManagement(adherentStolon);
            vm.Stolon = GetCurrentStolon();
            vm.ConsumerBills = _context.ConsumerBills
                                .Include(x => x.AdherentStolon)
                                .Include(x => x.AdherentStolon.Adherent)
                                .Include(x => x.AdherentStolon.Stolon)
                                .Where(x => x.State == BillState.Pending && x.AdherentStolon.StolonId == stolon.Id)
                                .OrderBy(x => x.AdherentStolon.Adherent.Id)
                                .AsNoTracking()
                                .ToList();
            vm.ProducerBills = _context.ProducerBills
                                .Include(x => x.AdherentStolon)
                                .Include(x => x.AdherentStolon.Adherent)
                                .Include(x => x.AdherentStolon.Stolon)
                                .Where(x => x.State != BillState.Paid && x.AdherentStolon.StolonId == stolon.Id)
                                .OrderBy(x => x.AdherentStolon.Adherent.Id)
                                .AsNoTracking()
                                .ToList();
            vm.StolonsBills = _context.StolonsBills.Include(x => x.Stolon).Where(x => x.StolonId == stolon.Id).ToList();
            vm.WeekStolonsBill = vm.StolonsBills.FirstOrDefault(x => x.BillNumber.EndsWith(DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear()));
            return View(vm);
        }

        // GET: UpdateConsumerBill
        [HttpPost, ActionName("GetBillsToPay")]
        public IActionResult GetBillsToPay()
        {
            Stolon stolon = GetCurrentStolon();
            var producerBillsToPay = _context.ProducerBills
                                   .Include(x => x.AdherentStolon)
                                   .Include(x => x.AdherentStolon.Adherent)
                                   .Include(x => x.AdherentStolon.Stolon)
                                   .Where(x => x.State == BillState.Delivered && x.AdherentStolon.StolonId == stolon.Id)
                                   .OrderBy(x => x.AdherentStolon.Adherent.Id)
                                   .AsNoTracking()
                                   .ToList();

            return Json(new VmProducersBills(GetActiveAdherentStolon(),producerBillsToPay));
        }

        // GET: CancelConsumerBill
        [HttpPost, ActionName("CancelConsumerBill")]
        public IActionResult CancelConsumerBill(Guid billId, string reason)
        {
            //Consumer
            ConsumerBill bill = _context.ConsumerBills.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x=>x.AdherentStolon.Stolon).Include(x=>x.BillEntries).ThenInclude(x=>x.ProducerBill).First(x => x.BillId == billId);
            string prodModificationReason = "Modification de la facture: " + DateTime.Now.ToString() + "\n\rRaison : Annulation du panier de l'adhérent "+bill.AdherentStolon.LocalId+", raison :" + reason + "\n\r\n\r";
            bill.ModificationReason += "Annulation du panier : " + DateTime.Now.ToString() + "\n\rRaison :" + reason + "\n\r\n\r";
            bill.HasBeenModified = true;
            bill.State = BillState.Cancelled;
            bill.HtmlBillContent = BillGenerator.GenerateHtmlBillContent(bill, _context);
            _context.SaveChanges();
            BillGenerator.GenerateBillPDF(bill);
            AuthMessageSender.SendEmail(bill.AdherentStolon.Stolon.Label,
                   bill.AdherentStolon.Adherent.Email,
                   bill.AdherentStolon.Adherent.Surname + " " + bill.AdherentStolon.Adherent.Name,
                   "Votre  commande de la semaine chez " + bill.AdherentStolon.Stolon.Label + "a été annulé (Commande " + bill.BillNumber + ")",
                   "Oops, il y a eu un petit problème avec votre commande. Elle a été annulé\n\rEn voici la raison : \n\r" + bill.ModificationReason + "\n\rToutes nos excuses pour ce désagrément.");



            //Producers
            foreach (var prodBill in bill.BillEntries.GroupBy(x=>x.ProducerBill))
            {
                var producerBill = _context.ProducerBills.Include(x => x.BillEntries).Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Stolon).Include(x => x.AdherentStolon.Adherent).First(x => x.BillId == prodBill.Key.BillId);
                producerBill.ModificationReason += prodModificationReason;
                producerBill.HasBeenModified = true;
                producerBill.HtmlBillContent = BillGenerator.GenerateHtmlBillContent(producerBill, _context);
                producerBill.HtmlOrderContent = BillGenerator.GenerateHtmlOrderContent(producerBill, _context);
                _context.SaveChanges();
                BillGenerator.GenerateOrderPDF(producerBill);
                //Send mail ?
                AuthMessageSender.SendEmail(producerBill.AdherentStolon.Stolon.Label,
                       producerBill.AdherentStolon.Adherent.Email,
                       producerBill.AdherentStolon.Adherent.CompanyName,
                       "Votre bon de commande de la semaine chez " + producerBill.AdherentStolon.Stolon.Label + "a été modifié (Bon de commande " + producerBill.BillNumber + ")",
                       "Oops, il y a eu un petit problème avec votre commande. Malheureusement tous les produits commandés ne sont pas disponible.\n\rVotre bon de commande a été modifiée.\n\rEn voici la raison : \n\r" + prodModificationReason + "\n\rToutes nos excuses pour ce désagrément.\n\r" + producerBill.HtmlBillContent);
            }

            //Stolon bill
            var stolonBillToModify = _context.StolonsBills.Include(x => x.BillEntries).ThenInclude(x => x.ProducerBill).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
                                                          .Include(x => x.BillEntries).ThenInclude(x => x.ConsumerBill).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
                                                          .Include(x => x.BillEntries).ThenInclude(x => x.ProductStock).ThenInclude(x => x.Product)
                                                          .First(x => x.BillEntries.Any(y => y.Id == bill.BillEntries.First().Id));
            stolonBillToModify.UpdateBillInfo();
            stolonBillToModify.HasBeenModified = true;
            stolonBillToModify.ModificationReason += "Modification des commandes : " + DateTime.Now.ToString() + "\n\rRaison : Annulation du panier de l'adhérent " + bill.AdherentStolon.LocalId + ", raison :" + reason + "\n\r\n\r";
            stolonBillToModify.HtmlBillContent = BillGenerator.GenerateHtmlContent(stolonBillToModify);
            BillGenerator.GeneratePDF(stolonBillToModify.HtmlBillContent, stolonBillToModify.GetStolonBillFilePath());
            _context.SaveChanges();
            return Json(true);
        }

        // GET: UpdateConsumerBill
        [HttpPost, ActionName("UpdateConsumerBill")]
        public IActionResult UpdateConsumerBill(Guid billId, int mode)
        {
            PaymentMode paymentMode = (PaymentMode)mode;
            ConsumerBill bill = _context.ConsumerBills.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).First(x => x.BillId == billId);
            bill.State = BillState.Paid;
            _context.Update(bill);
            if (paymentMode == PaymentMode.Token)
            {
                bill.AdherentStolon.Token -= bill.OrderAmount;
                _context.Update(bill.AdherentStolon);
            }
            //Transaction
            Transaction transaction = new Transaction(
                GetCurrentStolon(),
                TransactionType.Inbound,
                TransactionCategory.BillPayement,
                paymentMode == PaymentMode.Token ? 0 : bill.OrderAmount,
                "Paiement de la facture " + bill.BillNumber + " par " + bill.AdherentStolon.Adherent.Name + "( " + bill.AdherentStolon.LocalId + " ) en " + EnumHelper<PaymentMode>.GetDisplayValue(paymentMode));
            _context.Add(transaction);
            //Save
            _context.SaveChanges();
            return Json(true);
        }

        // GET: UpdateProducerBill
        [HttpPost, ActionName("UpdateProducerBill")]
        public IActionResult UpdateProducerBill(Guid billId, int state)
        {
            ProducerBill bill = _context.ProducerBills
                                .Include(x => x.AdherentStolon)
                                .Include(x => x.AdherentStolon.Adherent)
                                .First(x => x.BillId == billId);

            bill.State = (BillState)state;
            _context.Update(bill);

            if (bill.State == BillState.Paid)
            {
                //Transaction
                Transaction prodRefound = new Transaction(
                    GetCurrentStolon(),
                    Transaction.TransactionType.Outbound,
                    Transaction.TransactionCategory.ProducerRefound,
                    bill.BillAmount,
                    "Paiement de la facture " + bill.BillNumber + " à " + bill.AdherentStolon.Adherent.CompanyName + " ( " + bill.AdherentStolon.LocalId + " )");
                _context.Add(prodRefound);
                Transaction comitionInbound = new Transaction(
                    GetCurrentStolon(),
                    Transaction.TransactionType.Inbound,
                    Transaction.TransactionCategory.ProducersFee,
                    bill.FeeAmount,
                    "Encaissement de la commission de la facture " + bill.BillNumber + " de " + bill.AdherentStolon.Adherent.CompanyName + " ( " + bill.AdherentStolon.LocalId + " )");
                _context.Add(comitionInbound);
            }
            //Save
            _context.SaveChanges();
            //
            if (bill.State == BillState.Delivered)
            {
                //Generate bill in pdf
                BillGenerator.GenerateBillPDF(bill);
            }
            return Json(new VmProducerBill(GetActiveAdherentStolon(), bill));
        }

        //Debug and last resort utility method
        public string RegenerateOrders()
        {
            var stolon = GetCurrentStolon();
            var consumersBills = _context.ConsumerBills
                                .Include(x => x.AdherentStolon)
                                .Include(x => x.AdherentStolon.Adherent)
                                .Include(x => x.AdherentStolon.Stolon)
                                .OrderBy(x => x.AdherentStolon.Adherent.Id)
                                .ToList();
            var producersBills = _context.ProducerBills
                                .Include(x => x.AdherentStolon)
                                .Include(x => x.AdherentStolon.Adherent)
                                .Include(x => x.AdherentStolon.Stolon)
                                .OrderBy(x => x.AdherentStolon.Adherent.Id)
                                .ToList();

            var weekStolonBill = _context.StolonsBills
                            .Include(x => x.Stolon)
                            .Where(x => x.StolonId == stolon.Id)
                            .ToList();

            DateTime start = DateTime.Now;
            StringBuilder report = new StringBuilder();
            report.Append("Rapport de génération des commandes : ");
            report.AppendLine();
            Dictionary<ConsumerBill, bool> consumers = new Dictionary<ConsumerBill, bool>();
            consumersBills.ForEach(x => consumers.Add(x, BillGenerator.GenerateBillPDF(x)));
            Dictionary<ProducerBill, bool> producers = new Dictionary<ProducerBill, bool>();
            producersBills.ForEach(x => producers.Add(x, BillGenerator.GenerateOrderPDF(x)));
            Dictionary<StolonsBill, bool> weeks = new Dictionary<StolonsBill, bool>();
            weekStolonBill.ForEach(x => weeks.Add(x, BillGenerator.GeneratePDF(x.HtmlBillContent, x.GetStolonBillFilePath())));
            report.AppendLine("RESUME : ");
            report.AppendLine("Commandes consomateurs générées : " + consumers.Count(x => x.Value == true) + "/" + consumers.Count);
            report.AppendLine("Commandes producteurs générées : " + producers.Count(x => x.Value == true) + "/" + producers.Count);
            report.AppendLine("Commandes stolons générées : " + weeks.Count(x => x.Value == true) + "/" + weeks.Count);
            report.AppendLine("DETAILS : ");
            report.AppendLine("-- Consomateurs ok : ");
            consumers.Where(x => x.Value).ToList().ForEach(consumer => report.AppendLine(consumer.Key.AdherentStolon.LocalId + " " + consumer.Key.AdherentStolon.Adherent.Name.ToUpper() + " " + consumer.Key.AdherentStolon.Adherent.Surname));
            report.AppendLine("-- Consomateurs nok : ");
            consumers.Where(x => x.Value == false).ToList().ForEach(consumer => report.AppendLine(consumer.Key.AdherentStolon.LocalId + " " + consumer.Key.AdherentStolon.Adherent.Name.ToUpper() + " " + consumer.Key.AdherentStolon.Adherent.Surname));
            report.AppendLine("-- Producteurs ok : ");
            producers.Where(x => x.Value).ToList().ForEach(producer => report.AppendLine(producer.Key.AdherentStolon.LocalId + " " + producer.Key.AdherentStolon.Adherent.Name.ToUpper() + " " + producer.Key.AdherentStolon.Adherent.Surname));
            report.AppendLine("-- Producteurs nok : ");
            producers.Where(x => x.Value == false).ToList().ForEach(producer => report.AppendLine(producer.Key.AdherentStolon.LocalId + " " + producer.Key.AdherentStolon.Adherent.Name.ToUpper() + " " + producer.Key.AdherentStolon.Adherent.Surname));
            report.AppendLine("-- Semaines ok : ");
            weeks.Where(x => x.Value).ToList().ForEach(week => report.AppendLine(week.Key.BillNumber));
            report.AppendLine("-- Semaines nok : ");
            weeks.Where(x => x.Value == false).ToList().ForEach(week => report.AppendLine(week.Key.BillNumber));
            report.AppendLine();
            report?.AppendLine(" --Temps d'execution de la génération : " + (DateTime.Now - start));
            return report.ToString();
        }

        [HttpPost, ActionName("UpdateBillCorrection")]
        public bool UpdateBillCorrection(VmBillCorrection billCorrection)
        {
            billCorrection.Reason = "Modification le : " + DateTime.Now.ToString() + "\n\rRaison : " + billCorrection.Reason + "\n\r\n\r";
            ProducerBill producerBill = _context.ProducerBills.Include(x => x.BillEntries).ThenInclude(x=>x.ConsumerBill).ThenInclude(x=>x.AdherentStolon).Include(x => x.AdherentStolon).First(x => x.BillId == billCorrection.ProducerBillId);
            List<Guid?> modifiedBills = new List<Guid?>();
            foreach (var billQuantity in billCorrection.NewQuantities)
            {
                var billEntry = producerBill.BillEntries.First(x => x.Id == billQuantity.BillId);
                if (!modifiedBills.Any(x => x == billEntry.ConsumerBillId))
                    modifiedBills.Add(billEntry.ConsumerBillId);
                this.UpdateBillEntryStock(billEntry, billQuantity.Quantity);
            }
            _context.SaveChanges();

            //Producer bill
            producerBill = _context.ProducerBills.Include(x => x.BillEntries).Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Stolon).Include(x => x.AdherentStolon.Adherent).First(x => x.BillId == billCorrection.ProducerBillId);
            producerBill.BillEntries.ForEach(x => x.ProductStock = _context.ProductsStocks.Include(y => y.Product).First(stock => stock.Id == x.ProductStockId));
            producerBill.ModificationReason += billCorrection.Reason;
            producerBill.HasBeenModified = true;
            producerBill.HtmlBillContent = BillGenerator.GenerateHtmlBillContent(producerBill, _context);
            producerBill.HtmlOrderContent = BillGenerator.GenerateHtmlOrderContent(producerBill, _context);
            BillGenerator.GenerateBillPDF(producerBill);
            BillGenerator.GenerateOrderPDF(producerBill);
            _context.SaveChanges();
            //Consumers bills
            foreach (var billId in modifiedBills)
            {
                var billToModify = _context.ConsumerBills.Include(x => x.AdherentStolon.Adherent).First(x => x.BillId == billId);
                billToModify.ModificationReason += billCorrection.Reason;
                billToModify.HasBeenModified = true;
                billToModify.HtmlBillContent = BillGenerator.GenerateHtmlBillContent(billToModify, _context);
                //Envoie mail user
                BillGenerator.GenerateBillPDF(billToModify);
                AuthMessageSender.SendEmail(billToModify.AdherentStolon.Stolon.Label,
                            billToModify.AdherentStolon.Adherent.Email,
                            billToModify.AdherentStolon.Adherent.CompanyName,
                            "Votre bon de commande de la semaine chez " + billToModify.AdherentStolon.Stolon.Label + "a été modifié (Bon de commande " + producerBill.BillNumber + ")",
                            "Oops, il y a eu un petit problème avec votre commande. Malheureusement tous les produits commandés ne sont pas disponible.\n\rVotre commande a été modifiée.\n\rEn voici la raison : \n\r" + billCorrection.Reason + "\n\rToutes nos excuses pour ce désagrément.\n\r" + billToModify.HtmlBillContent);
            }
            _context.SaveChanges();
            //Stolon bill
            var stolonBillToModify = _context.StolonsBills.Include(x => x.BillEntries).ThenInclude(x => x.ProducerBill).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
                                                          .Include(x => x.BillEntries).ThenInclude(x => x.ConsumerBill).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
                                                          .Include(x => x.BillEntries).ThenInclude(x => x.ProductStock).ThenInclude(x => x.Product)
                                                          .First(x => x.BillEntries.Any(y => y.Id == producerBill.BillEntries.First().Id));
            stolonBillToModify.UpdateBillInfo();
            stolonBillToModify.HasBeenModified = true;
            stolonBillToModify.ModificationReason += billCorrection.Reason;
            stolonBillToModify.HtmlBillContent = BillGenerator.GenerateHtmlContent(stolonBillToModify);
            BillGenerator.GeneratePDF(stolonBillToModify.HtmlBillContent, stolonBillToModify.GetStolonBillFilePath());
            _context.SaveChanges();
            return true;
        }

        //Updates the stock of a productStock after the billEntry has been modified
        private void UpdateBillEntryStock(BillEntry billEntry, int newQuantity)
        {
            var productStock = _context.ProductsStocks.Include(x => x.Product).First(x => x.Id == billEntry.ProductStockId);
            var diffQty = billEntry.Quantity - newQuantity;

            if (productStock.Product.StockManagement == Product.StockType.Fixed)
            {
                productStock.RemainingStock = productStock.RemainingStock + diffQty;
            }
            billEntry.Quantity = newQuantity;
            billEntry.HasBeenModified = true;
        }

        // GET: ShowBill
        public IActionResult ShowStolonsBill(string id)
        {
            StolonsBill bill = _context.StolonsBills.FirstOrDefault(x => x.BillNumber == id);
            if (bill != null)
                return View(bill);
            //Bill not found
            return View(null);
        }

        [HttpGet, ActionName("ProducerBill"), Route("api/producerBill")]
        public IActionResult ProducerBill(string billId)
        {
            IBill bill = _context.ProducerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).AsNoTracking().First(x => x.BillId.ToString() == billId);
            bill.BillEntries = _context.BillEntrys.Include(x => x.ProductStock).Where(x => x.ProducerBillId.ToString() == billId).AsNoTracking().ToList();

            foreach (BillEntry billEntry in bill.BillEntries)
            {
                billEntry.ConsumerBill = _context.ConsumerBills.Include(x => x.BillEntries).ThenInclude(x => x.ProductStock).ThenInclude(x => x.Product).Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).AsNoTracking().First(x => x.BillId == billEntry.ConsumerBillId);
                billEntry.ProductStock = _context.ProductsStocks.First(x => x.Id == billEntry.ProductStockId);
                billEntry.ProductStock.Product = _context.Products.First(x => x.Id == billEntry.ProductStock.ProductId);
            }
            return Json(bill);
        }

        // [HttpGet, ActionName("CustomersForBill"), Route("api/billCustomers")]
        // public string AddToBasket(string billId)
        // {
        //     IBill bill = _context.ProducerBills.Include(x => x.BillEntries).ThenInclude(x => x.ProductStock).ThenInclude(x => x.Product).Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.AdherentStolon.Stolon).First(x => x.BillId.ToString() == billId);
        //     return JsonConvert.SerializeObject(bill, Formatting.Indented, new JsonSerializerSettings()
        //     {
        // 	ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //     });
        // }
    }
}
