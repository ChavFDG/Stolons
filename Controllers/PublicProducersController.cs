using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Stolons.Controllers
{
    public class PublicProducersController : BaseController
    {

        public PublicProducersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {
        }

        // GET: PublicProducers
        [AllowAnonymous]
        public IActionResult Index()
        {
            Stolon stolon = GetCurrentStolon();
	    var producers = _context.AdherentStolons
		.Include(x => x.Adherent)
		.Where(x => x.IsProducer && x.StolonId == stolon.Id)
		.AsNoTracking()
		.ToList();

            return View(producers);
        }

        [AllowAnonymous]
        [HttpGet, ActionName("Producers"), Route("api/producers")]
        public IActionResult JsonProductsStocks()
        {
            Stolon stolon = GetCurrentStolon();
	    var producers = _context.AdherentStolons
		.Include(x => x.Adherent)
		.Where(x => x.IsProducer && x.StolonId == stolon.Id)
		.AsNoTracking()
		.ToList();

            return Json(producers);
        }

	[AllowAnonymous]
        [HttpGet, ActionName("PublicProducerProducts"), Route("api/publicProducerProducts")]
        public IActionResult JsonPublicProductsStocks(Guid producerStolonId)
        {
	    if (producerStolonId == null)
	    {
		return Json(new {});
	    }
            var productsStocks = _context.ProductsStocks
		.Include(x => x.Product)
		.ThenInclude(x => x.Familly)
		.ThenInclude(x => x.Type)
		.Where(x => x.AdherentStolonId == producerStolonId)
		.AsNoTracking()
		.ToList();
            return Json(productsStocks);
        }
    }
}
