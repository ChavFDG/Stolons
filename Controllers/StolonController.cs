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
    public class StolonController : StolonsAdministrationController
    {

        public StolonController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context, environment, userManager, signInManager, serviceProvider)
        {

        }

        public override IActionResult Index()
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            var activeAdherentStolon = GetActiveAdherentStolon();
            ViewData["Controller"] = "Stolon";
            return View(new StolonViewModel(activeAdherentStolon, activeAdherentStolon.Stolon));
        }

	[AllowAnonymous]
	[Route("api/stolons")]
	public IActionResult JsonGetStolons()
	{
	    var stolons = _context.Stolons.AsNoTracking().ToList();
	    foreach (var stolon in stolons)
	    {
		stolon.Producers = _context.AdherentStolons
		    .Where(x => x.IsProducer && x.StolonId == stolon.Id)
		    .AsNoTracking()
		    .ToList();
		stolon.Products = _context.ProductsStocks
		    .Where(x => x.AdherentStolon.StolonId == stolon.Id)
		    .AsNoTracking()
		    .ToList();
		stolon.StringPickupTime = stolon.GetStringPickUpTime();
	    }
	    return Json(stolons);
	}
    }
}
