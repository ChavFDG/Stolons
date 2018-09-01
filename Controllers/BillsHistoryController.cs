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
using Stolons.ViewModels.BillHistory;

namespace Stolons.Controllers
{
    public class BillsHistoryController : BaseController
    {
        public BillsHistoryController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        // GET: 
        [Authorize()]
        public IActionResult Index(string id)
        {
            AdherentStolon activeAdherentStolon = GetActiveAdherentStolon();
            AdherentStolon adherentStolon ;
            List<ProducerBill> bills = new List<ProducerBill>();
            if (!String.IsNullOrWhiteSpace(id))
            {
                adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).First(x => x.Id.ToString() == id);
                _context.ProducerBills
                    .Include(x => x.AdherentStolon)
                    .Include(x => x.AdherentStolon.Adherent)
                    .Include(x => x.AdherentStolon.Stolon)
                    .Where(x => x.AdherentStolon.Id == adherentStolon.Id)
                    .AsNoTracking()
                    .ToList()
                    .ForEach(x => bills.Add(x));
            }
            else
            {
                adherentStolon = activeAdherentStolon;
                _context.ProducerBills
                .Include(x => x.AdherentStolon)
                .Include(x => x.AdherentStolon.Adherent)
                .Include(x => x.AdherentStolon.Stolon)
                .Where(x => x.AdherentStolon.AdherentId == adherentStolon.AdherentId)
                .AsNoTracking()
                .ToList()
                .ForEach(x => bills.Add(x));
            }

            
            return View(new BillsViewModel(activeAdherentStolon, adherentStolon, bills));
        }


        // GET: ShowBill
        public IActionResult ShowBill(string id)
        {
            IBill bill = _context.ConsumerBills.FirstOrDefault(x => x.BillNumber == id);

            if (bill != null)
                return View(bill);
            bill = _context.ProducerBills
        .Include(x => x.AdherentStolon)
        .Include(x => x.AdherentStolon.Adherent)
        .AsNoTracking()
        .FirstOrDefault(x => x.BillNumber == id);
            if (bill != null)
                return View(bill);
            //Bill not found
            return View(null);
        }
    }
}
