using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using static Stolons.Configurations;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Net.Http.Headers;
using Stolons.Helpers;
using Stolons.ViewModels.Stolons;
using Stolons.ViewModels.Adherents;

namespace Stolons.Controllers
{
    public class StolonsController : StolonsAdministrationController
    {

        public StolonsController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context, environment, userManager, signInManager, serviceProvider)
        {

        }

        // GET: Stolons
        public override IActionResult Index()
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            AdherentStolon activeAdherentStolon = GetActiveAdherentStolon();
            List<AdherentsViewModel> adherentsViewModel = new List<AdherentsViewModel>();
            foreach(var stolon in _context.Stolons.ToList())
            {
                adherentsViewModel.Add(new AdherentsViewModel(activeAdherentStolon, stolon, _context.Sympathizers.Where(x => x.StolonId == stolon.Id).ToList(), _context.AdherentStolons.Include(x=>x.Stolon).Include(x=>x.Adherent).Where(x => x.StolonId == stolon.Id).ToList()));
            }
            ViewData["Controller"] = "Stolons";
            return View(new StolonsViewModel(GetActiveAdherentStolon(), adherentsViewModel));
        }
    }
}
