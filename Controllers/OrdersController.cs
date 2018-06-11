using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using Stolons.Models.Users;
using System.Collections.Generic;

namespace Stolons.Controllers
{
    public class OrdersController : BaseController
    {
        public OrdersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        // GET: 
        [Authorize()]
        public async Task<IActionResult> Index()
        {
            Adherent stolonsUser = await this.GetCurrentAdherentAsync();
            List<ConsumerBill> bills = new List<ConsumerBill>();
            _context.ConsumerBills
		.Include(x => x.AdherentStolon)
		.Include(x => x.AdherentStolon.Adherent)
		.Include(x => x.AdherentStolon.Stolon)
		.Where(x => x.AdherentStolon.Adherent.Email == stolonsUser.Email)
		.AsNoTracking()
		.ToList()
		.ForEach(x => bills.Add(x));
            return View(bills);
        }

        // GET: ShowBill
        public IActionResult ShowOrder(string id)
        {
            IBill bill = _context.ConsumerBills.FirstOrDefault(x => x.BillNumber == id);
            if (bill != null)
                return View(bill);
            bill = _context.ProducerBills.Include(x => x.AdherentStolon).FirstOrDefault(x => x.OrderNumber == id);
            if (bill != null)
                return View(bill);
            //Bill not found
            return View(null);
        }        
    }
}
