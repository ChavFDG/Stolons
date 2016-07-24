using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using static Stolons.Configurations;

namespace Stolons.Controllers
{
    public class ApplicationConfigController : BaseController
    {
        private ApplicationDbContext _context;

        public ApplicationConfigController(ApplicationDbContext context,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _context = context;    
        }

        [Authorize(Roles = Role_Administrator)]
        // GET: ApplicationConfig
        public IActionResult Index()
        {
            return View(_context.ApplicationConfig.First());
        }

        [Authorize(Roles = Role_Administrator)]
        // GET: ApplicationConfig/Edit/5
        public IActionResult Edit()
        {
            return View(_context.ApplicationConfig.First());
        }

	[HttpGet, ActionName("CurrentMode"), Route("api/currentMode")]
	public string JsonCurrentMode() {
	    return JsonConvert.SerializeObject(Configurations.Mode, Formatting.Indented, new JsonSerializerSettings() {
		    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});
	}

        // POST: ApplicationConfig/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role_Administrator)]
        public IActionResult Edit(ApplicationConfig applicationConfig)
        {
            if (ModelState.IsValid)
            {
                Configurations.ApplicationConfig = applicationConfig;
                _context.Update(applicationConfig);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(applicationConfig);
        }
    }
}
