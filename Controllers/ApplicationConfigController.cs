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
using Stolons.Tools;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Net.Http.Headers;

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

        [ActionName("TestChromiumPath")]
        public bool TestChromiumPath()
        {
            return System.IO.File.Exists(Application.ChromiumFullPath);
        }

        [ActionName("TestPDF")]
        public FileResult TestPDF()
        {
            string tempFilePath = Path.Combine(Configurations.Environment.WebRootPath, "temp", "test.pdf");
            BillGenerator.GeneratePDF("Test de la génération d'un PDF", tempFilePath);

            byte[] fileBytes = System.IO.File.ReadAllBytes(tempFilePath);
            return File(fileBytes, "application/pdf", "test.pdf");

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
                ApplicationConfig appConfig = _context.ApplicationConfig.First();
                appConfig.ChromiumFullPath = applicationConfig.ChromiumFullPath;
                appConfig.ContactMailAddress = applicationConfig.ContactMailAddress;
                appConfig.ContactPhoneNumber = applicationConfig.ContactPhoneNumber;
                appConfig.IsInMaintenance = applicationConfig.IsInMaintenance;
                appConfig.MailAddress = applicationConfig.MailAddress;
                appConfig.MailPassword = applicationConfig.MailPassword;
                appConfig.MailPort = applicationConfig.MailPort;
                appConfig.MailSmtp = applicationConfig.MailSmtp;
                appConfig.MaintenanceMessage = applicationConfig.MaintenanceMessage;
                appConfig.StolonsLabel = applicationConfig.StolonsLabel;
                _context.Update(appConfig);
                _context.SaveChanges();
                Application = appConfig;
                return RedirectToAction("Index");
            }
            return View(applicationConfig);
        }
       
    }
}
