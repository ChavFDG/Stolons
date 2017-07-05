using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using static Stolons.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;

//TODO tout à revoir ici. On a d'un coté la configuration global de l'application et de l'autre la configuration d'un Stolon

namespace Stolons.Controllers
{
    public class ApplicationConfigController : BaseController
    {

        public ApplicationConfigController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }
        
        // GET: Stolon
        public IActionResult Index()
        {
            if(!AuthorizedWebAdmin())
                return Unauthorized();

            return View(_context.ApplicationConfig.First());
        }
        
        // GET: Stolon/Edit/5
        public IActionResult Edit()
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            return View(_context.ApplicationConfig.First());
        }

	    [HttpGet, ActionName("CurrentMode"), Route("api/currentMode")]
	    public string JsonCurrentMode() {
	        return JsonConvert.SerializeObject(GetCurrentStolon().GetMode(), Formatting.Indented, new JsonSerializerSettings() {
		        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			    });
	    }

        // POST: Stolon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationConfig applicationConfig)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            if (ModelState.IsValid)
            {
                //Configurations.Application = applicationConfig;
                _context.Update(applicationConfig);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(applicationConfig);
        }
       
    }
}
