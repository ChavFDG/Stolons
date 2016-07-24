using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System;

namespace Stolons.Controllers
{
    public class PublicProducersController : BaseController
    {
        private ApplicationDbContext _context;

        public PublicProducersController(ApplicationDbContext context,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _context = context;    
        }

        // GET: PublicProducers
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_context.Producers.ToList());
        }

	[AllowAnonymous]
	[HttpGet, ActionName("Producers"), Route("api/producers")]
	public string JsonProducts() {
	    var producers = _context.Producers.ToList();

	    return JsonConvert.SerializeObject(producers, Formatting.Indented, new JsonSerializerSettings() {
		    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			});
	}
    }
}
