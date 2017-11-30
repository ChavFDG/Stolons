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
            return View(_context.Adherents.Include(x => x.AdherentStolons).Where(x => x.AdherentStolons.Any(adhSto => adhSto.IsProducer && adhSto.StolonId == stolon.Id)).ToList());
        }

        [AllowAnonymous]
        [HttpGet, ActionName("Producers"), Route("api/producers")]
        public IActionResult JsonProductsStocks()
        {
            Stolon stolon = GetCurrentStolon();
            var producers = _context.Adherents.Include(x => x.AdherentStolons).AsNoTracking().Where(x => x.AdherentStolons.Any(adhSto => adhSto.IsProducer && adhSto.StolonId == stolon.Id)).ToList();

            return Json(producers);
        }
    }
}
