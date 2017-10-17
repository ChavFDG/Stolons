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
            vm.ConsumerBills = _context.ConsumerBills.Include(x=>x.AdherentStolon).ThenInclude(x=>x.Adherent).Include(x=>x.AdherentStolon).ThenInclude(x=>x.Stolon).Where(x => x.State == BillState.Pending && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x=>x.Adherent.Id).ToList();
            vm.ProducerBills = _context.ProducerBills.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.AdherentStolon).ThenInclude(x => x.Stolon).Where(x => x.State != BillState.Paid && x.AdherentStolon.StolonId == stolon.Id).OrderBy(x => x.Adherent.Id).ToList();
            vm.StolonsBills = _context.StolonsBills.Include(x=>x.Stolon).Where(x => x.StolonId == stolon.Id).ToList();
            vm.WeekStolonsBill = vm.StolonsBills.FirstOrDefault(x => x.BillNumber.EndsWith(DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear()));
            return View(vm);
        }

        // GET: UpdateConsumerBill
        public IActionResult UpdateConsumerBill(string billNumber, PaymentMode paymentMode)
        {
            ConsumerBill bill = _context.ConsumerBills.Include(x => x.AdherentStolon).First(x => x.BillNumber == billNumber);
            bill.State = BillState.Paid;
            //_context.Update(bill);
            //Transaction
            Transaction transaction = new Transaction(
                GetCurrentStolon(),
                TransactionType.Inbound,
                TransactionCategory.BillPayement,
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
            ProducerBill bill = _context.ProducerBills.Include(x=>x.AdherentStolon).First(x => x.BillNumber == billNumber);
            bill.State++;
            _context.Update(bill);
            if(bill.State == BillState.Paid)
            {
                //Transaction
                Transaction prodRefound = new Transaction(
                    GetCurrentStolon(),
                    Transaction.TransactionType.Outbound,
                    Transaction.TransactionCategory.ProducerRefound,
                    bill.BillAmount,
                    "Paiement de la facture " + bill.BillNumber + " à " + bill.Adherent.CompanyName + " ( " + bill.Adherent.Id + " )");
                _context.Add(prodRefound);
                Transaction comitionInbound = new Transaction(
                    GetCurrentStolon(),
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

	[HttpGet, ActionName("ProducerBill"), Route("api/producerBill")]
        public string ProducerBill(string billId)
	{
	    //Entity framework ? Hum... ouai
            IBill bill = _context.ProducerBills.Include(x => x.BillEntries).ThenInclude(x => x.ConsumerBill)
		.Include(x => x.BillEntries).ThenInclude(x => x.ProductStock).ThenInclude(x => x.Product)
		.Include(x => x.BillEntries).ThenInclude(x => x.ConsumerBill).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
		.Include(x => x.BillEntries).ThenInclude(x => x.ConsumerBill).ThenInclude(x => x.BillEntries)
		.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent)
		.First(x => x.BillId.ToString() == billId);
	    return JsonConvert.SerializeObject(bill, Formatting.Indented, new JsonSerializerSettings()
	    {
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore
	    });
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
