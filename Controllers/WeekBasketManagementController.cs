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

        // GET: Bills
        public IActionResult Index()
        {
            Stolon stolon = GetCurrentStolon();
            var adherentStolon = GetActiveAdherentStolon();
            VmWeekBasketManagement vm = new VmWeekBasketManagement(adherentStolon);
            vm.Stolon = GetCurrentStolon();
            vm.ConsumerBills = _context.ConsumerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.AdherentStolon).ThenInclude(x => x.Stolon).Where(x => x.State == BillState.Pending && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x => x.Adherent.Id).ToList();
            vm.ProducerBills = _context.ProducerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.AdherentStolon).ThenInclude(x => x.Stolon).Where(x => x.State != BillState.Paid && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x => x.Adherent.Id).ToList();
            vm.StolonsBills = _context.StolonsBills.Include(x => x.Stolon).Where(x => x.StolonId == stolon.Id).ToList();
            vm.WeekStolonsBill = vm.StolonsBills.FirstOrDefault(x => x.BillNumber.EndsWith(DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear()));
            return View(vm);
        }

        // GET: UpdateConsumerBill
        public IActionResult UpdateConsumerBill(string billNumber, PaymentMode paymentMode)
        {
            ConsumerBill bill = _context.ConsumerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).First(x => x.BillNumber == billNumber);
            bill.State = BillState.Paid;
            _context.Update(bill);
            if(paymentMode == PaymentMode.Token)
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
                "Paiement de la facture " + bill.BillNumber + " par " + bill.Adherent.Name + "( " + bill.AdherentStolon.LocalId + " ) en " + EnumHelper<PaymentMode>.GetDisplayValue(paymentMode));
            _context.Add(transaction);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: UpdateProducerBill
        public IActionResult UpdateProducerBill(string billNumber)
        {
            ProducerBill bill = _context.ProducerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).First(x => x.BillNumber == billNumber);
            bill.State++;
            _context.Update(bill);
            if (bill.State == BillState.Paid)
            {
                //Transaction
                Transaction prodRefound = new Transaction(
                    GetCurrentStolon(),
                    Transaction.TransactionType.Outbound,
                    Transaction.TransactionCategory.ProducerRefound,
                    bill.BillAmount,
                    "Paiement de la facture " + bill.BillNumber + " à " + bill.Adherent.CompanyName + " ( " + bill.AdherentStolon.LocalId + " )");
                _context.Add(prodRefound);
                Transaction comitionInbound = new Transaction(
                    GetCurrentStolon(),
                    Transaction.TransactionType.Inbound,
                    Transaction.TransactionCategory.ProducersFee,
                    bill.FeeAmount,
                    "Encaissement de la commission de la facture " + bill.BillNumber + " de " + bill.Adherent.CompanyName + " ( " + bill.AdherentStolon.LocalId + " )");
                _context.Add(comitionInbound);
            }
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public string RegenerateOrders()
        {
            var stolon = GetCurrentStolon();
            var consumersBills = _context.ConsumerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.AdherentStolon).ThenInclude(x => x.Stolon).Where(x => x.State == BillState.Pending && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x => x.Adherent.Id).ToList();
            var producersBills = _context.ProducerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.AdherentStolon).ThenInclude(x => x.Stolon).Where(x => x.State != BillState.Paid && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x => x.Adherent.Id).ToList();
            var weekStolonBill = _context.StolonsBills.Include(x => x.Stolon).Where(x => x.StolonId == stolon.Id).ToList().FirstOrDefault(x => x.BillNumber.EndsWith(DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear()));
            DateTime start = DateTime.Now;
            StringBuilder report = new StringBuilder();
            report.Append("Rapport de génération des commandes : ");
            report.AppendLine("- stolon : " + stolon.Label);
            report.AppendLine("- annnée : " + DateTime.Now.Year);
            report.AppendLine("- semaine : " + DateTime.Now.GetIso8601WeekOfYear());
            report.AppendLine();
            Dictionary<ConsumerBill, bool> consumers = new Dictionary<ConsumerBill, bool>();
            consumersBills.ForEach(x => consumers.Add(x, BillGenerator.GenerateBillPDF(x)));
            Dictionary<ProducerBill, bool> producers = new Dictionary<ProducerBill, bool>();
            producersBills.ForEach(x => producers.Add(x, BillGenerator.GenerateOrderPDF(x)));
            bool stolonBill = BillGenerator.GeneratePDF(weekStolonBill.HtmlBillContent, weekStolonBill.FilePath);
            report.AppendLine("RESUME : ");
            report.AppendLine("Commandes consomateurs générés : " + consumers.Count(x => x.Value == true) + "/" + consumers.Count);
            report.AppendLine("Commandes producteurs générés : " + producers.Count(x => x.Value == true) + "/" + producers.Count);
            report.AppendLine("Commandes stolons généré : " + stolonBill);
            report.AppendLine("DETAILS : ");
            report.AppendLine("-- Consomateurs ok : ");
            consumers.Where(x => x.Value).ToList().ForEach(consumer => report.AppendLine(consumer.Key.AdherentStolon.LocalId + " " + consumer.Key.Adherent.Name.ToUpper() + " " + consumer.Key.Adherent.Surname));
            report.AppendLine("-- Consomateurs nok : ");
            consumers.Where(x => x.Value == false).ToList().ForEach(consumer => report.AppendLine(consumer.Key.AdherentStolon.LocalId + " " + consumer.Key.Adherent.Name.ToUpper() + " " + consumer.Key.Adherent.Surname));
            report.AppendLine("-- Producteurs ok : ");
            producers.Where(x => x.Value).ToList().ForEach(producer => report.AppendLine(producer.Key.AdherentStolon.LocalId + " " + producer.Key.Adherent.Name.ToUpper() + " " + producer.Key.Adherent.Surname));
            report.AppendLine("-- Producteurs nok : ");
            producers.Where(x => x.Value == false).ToList().ForEach(producer => report.AppendLine(producer.Key.AdherentStolon.LocalId + " " + producer.Key.Adherent.Name.ToUpper() + " " + producer.Key.Adherent.Surname));
            report.AppendLine();
            report?.AppendLine(" --Temps d'exécutions de la générations : " + (DateTime.Now - start));
            return report.ToString();
        }

        [HttpPost, ActionName("UpdateBillCorrection")]
        public bool UpdateBillCorrection(VmBillCorrection billCorrection)
        {
            try
            {
                billCorrection.Reason = "Modification le : " + DateTime.Now.ToString() + "\n\rRaison : " + billCorrection.Reason + "\n\r\n\r";
                ProducerBill bill = _context.ProducerBills.Include(x => x.BillEntries).Include(x => x.AdherentStolon).First(x => x.BillId == billCorrection.ProducerBillId);
                bill.ModificationReason = billCorrection.Reason;
                bill.HasBeenModified = true;
                List<Guid?> modifiedBills = new List<Guid?>();
                foreach (var billQuantity in billCorrection.NewQuantities)
                {
                    var billEntry = bill.BillEntries.First(x => x.Id == billQuantity.BillId);
                    billEntry.Quantity = billQuantity.Quantity;
                    billEntry.HasBeenModified = true;
                    if (!modifiedBills.Any(x => x == billEntry.ConsumerBillId))
                        modifiedBills.Add(billEntry.ConsumerBillId);
                }
                _context.SaveChanges();
                bill = _context.ProducerBills.Include(x => x.BillEntries).Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Stolon).Include(x => x.AdherentStolon.Adherent).First(x => x.BillId == billCorrection.ProducerBillId);
                bill.BillEntries.ForEach(x => x.ProductStock = _context.ProductsStocks.Include(y => y.Product).First(stock => stock.Id == x.ProductStockId));
                bill.HtmlBillContent = BillGenerator.GenerateHtmlBillContent(bill, _context);
                BillGenerator.GenerateBillPDF(bill);
                foreach (var billId in modifiedBills)
                {
                    var billToModify = _context.ConsumerBills.First(x => x.BillId == billId);
                    billToModify.ModificationReason += billCorrection.Reason;
                    billToModify.HasBeenModified = true;
                }
                _context.SaveChanges();

            }
            catch (Exception except)
            {
                return false;
            }
            return true;
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
