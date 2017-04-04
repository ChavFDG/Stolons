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
            VmWeekBasketManagement vm = new VmWeekBasketManagement();
            vm.Stolon = GetCurrentStolon();
            vm.ConsumerBills = _context.ConsumerBills.Include(x=>x.AdherentStolon).ThenInclude(x=>x.Adherent).Include(x=>x.AdherentStolon).ThenInclude(x=>x.Stolon).Where(x => x.State == BillState.Pending && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x=>x.Adherent.Id).ToList();
            vm.ProducerBills = _context.ProducerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.AdherentStolon).ThenInclude(x => x.Stolon).Where(x => x.State != BillState.Paid && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x => x.Adherent.Id).ToList();
            vm.StolonsBills = _context.StolonsBills.Include(x=>x.Stolon).Where(x => x.StolonId == stolon.Id).ToList();
            vm.WeekStolonsBill = vm.StolonsBills.FirstOrDefault(x => x.BillNumber == DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear());
            return View(vm);
        }

        // GET: UpdateConsumerBill
        public IActionResult UpdateConsumerBill(string billNumber, PaymentMode paymentMode)
        {
            ConsumerBill bill = _context.ConsumerBills.Include(x => x.Adherent).First(x => x.BillNumber == billNumber);
            bill.State = BillState.Paid;
            //_context.Update(bill);
            //Transaction
            Transaction transaction = new Transaction(
                Transaction.TransactionType.Inbound,
                Transaction.TransactionCategory.BillPayement,
                paymentMode == PaymentMode.Token ? 0 : bill.OrderAmount,
                "Paiement de la facture " + bill.BillNumber + " par " + bill.Adherent.Name + "( " + bill.Adherent.Id + " ) en " + EnumHelper<PaymentMode>.GetDisplayValue(paymentMode));
            //_context.Add(transaction);
            //Save
           // _context.SaveChanges();
            return RedirectToAction("Index");
        }
        // GET: UpdateProducerBill
        public IActionResult UpdateProducerBill(string billNumber)
        {
            ProducerBill bill = _context.ProducerBills.Include(x=>x.Adherent).First(x => x.BillNumber == billNumber);
            bill.State++;
            _context.Update(bill);
            if(bill.State == BillState.Paid)
            {
                //Transaction
                Transaction prodRefound = new Transaction(
                    Transaction.TransactionType.Outbound,
                    Transaction.TransactionCategory.ProducerRefound,
                    bill.BillAmount,
                    "Paiement de la facture " + bill.BillNumber + " à " + bill.Adherent.CompanyName + " ( " + bill.Adherent.Id + " )");
                _context.Add(prodRefound);
                Transaction comitionInbound = new Transaction(
                    Transaction.TransactionType.Inbound,
                    Transaction.TransactionCategory.ProducersFee,
                    bill.FeeAmount,
                    "Encaissement de la commission de la facture " + bill.BillNumber + " de " + bill.Adherent.CompanyName+ " ( " + bill.Adherent.Id + " )");
                _context.Add(comitionInbound);
            }
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: ShowBill
        public IActionResult ShowStolonsBill(string id)
        {
            StolonsBill bill = _context.StolonsBills.FirstOrDefault(x=>x.BillNumber == id);
            if (bill != null)
                return View(bill);
            //Bill not found
            return View(null);
        }
    }
}
