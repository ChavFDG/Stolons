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
using Stolons.ViewModels.PublicProducers;

namespace Stolons.Controllers
{
    public class ProducersController : BaseController
    {

        public ProducersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {
        }

        // GET: PublicProducers
        [AllowAnonymous]
        [Route("Producers/{id?}")]
        public IActionResult Index()
        {
            Stolon stolon = GetCurrentStolon();
            var producers = _context.AdherentStolons
            .Include(x => x.Adherent)
            .Where(x => x.IsProducer && x.StolonId == stolon.Id)
            .AsNoTracking()
            .ToList();

            int totalProducts = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).Count(x => x.AdherentStolon.StolonId == stolon.Id && !x.Product.IsArchive);
            return View(new PublicProducersViewModel(producers, totalProducts));
        }

        [AllowAnonymous]
        [HttpGet, ActionName("Producers"), Route("api/producers")]
        public IActionResult JsonProductsStocks(Guid? stolonId)
        {
	    Stolon stolon;
	    if (stolonId != null)
	    {
		stolon = _context.Stolons.AsNoTracking().FirstOrDefault(x => x.Id == stolonId);
	    }
	    else
	    {
		stolon = GetCurrentStolon();
	    }
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
                return Json(new { });
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
