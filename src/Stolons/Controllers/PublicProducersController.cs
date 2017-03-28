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
            return View(_context.Adherents.Where(x => x.IsProducer).ToList());
        }

	[AllowAnonymous]
	[HttpGet, ActionName("Producers"), Route("api/producers")]
	public string JsonProducts() {
	    var producers = _context.Adherents.Where(x=>x.IsProducer).ToList();

	    return JsonConvert.SerializeObject(producers, Formatting.Indented, new JsonSerializerSettings() {
		    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			});
	}
    }
}
