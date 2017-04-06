using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Stolons.Helpers;
using Stolons.ViewModels.Sympathizers;
using Stolons.Models.Users;
using Stolons.Models.Transactions;

namespace Stolons.Controllers
{
    public class SympathizersController : BaseController
    {
        public SympathizersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider,userManager,context,environment,signInManager)
        {

        }
        
        // GET: Sympathizer
        public IActionResult Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new SympathizersViewModel(GetActiveAdherentStolon(), _context.Sympathizers.Include(x=>x.Stolon).Where(x=>x.StolonId == GetCurrentStolon().Id) .ToList()));
        }
        
        // GET: Sympathizer/Details/5
        public IActionResult Details(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            if (sympathizer == null)
            {
                return NotFound();
            }
            return View(new SympathizerViewModel(GetActiveAdherentStolon(), sympathizer));
        }
        
        // GET: Sympathizer/PartialDetails/5
        public IActionResult PartialDetails(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            Sympathizer Sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            if (Sympathizer == null)
            {
                return NotFound();
            }
            return PartialView(new SympathizerViewModel(GetActiveAdherentStolon(), Sympathizer));
        }

        // GET: Sympathizer/Create
        public IActionResult Create()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new SympathizerViewModel(GetActiveAdherentStolon(), new Sympathizer()));
        }

        // POST: Sympathizer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SympathizerViewModel vmSympathizer)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                #region Creating Sympathizer
                string fileName = Configurations.DefaultFileName;

                //Setting value for creation
                vmSympathizer.Sympathizer.StolonId = GetCurrentStolon().Id;
                vmSympathizer.Sympathizer.RegistrationDate = DateTime.Now;
                _context.Sympathizers.Add(vmSympathizer.Sympathizer);
                #endregion Creating Sympathizer

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vmSympathizer);
        }

        // GET: Sympathizer/Edit/5
        public IActionResult Edit(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            if (sympathizer == null)
            {
                return NotFound();
            }

            return View(new SympathizerViewModel(GetActiveAdherentStolon(), sympathizer));
        }

        // POST: Sympathizer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SympathizerViewModel vmSympathizer)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {        
                _context.Update(vmSympathizer.Sympathizer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmSympathizer);
        }

        // GET: Sympathizer/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            if (sympathizer == null)
            {
                return NotFound();
            }
            return View(new SympathizerViewModel(GetActiveAdherentStolon(), sympathizer));
        }

        // POST: Sympathizer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);           
            _context.Sympathizers.Remove(sympathizer);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// PaySympathiserSubscription
        /// </summary>
        /// <param name="id">Sympathizer ID</param>
        /// <returns></returns>
        public IActionResult PaySympathiserSubscription(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            Transaction transaction = new Transaction();
            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            Stolon currentStolon = GetCurrentStolon();
            sympathizer.SubscriptionPaid = true;
            transaction.Amount = currentStolon.SympathizerSubscription;
            //Add a transaction
            transaction.Stolon = currentStolon;
            transaction.AddedAutomaticly = true;
            transaction.Date = DateTime.Now;
            transaction.Type = Transaction.TransactionType.Inbound;
            transaction.Category = Transaction.TransactionCategory.Subscription;
            transaction.Description = "Paiement de la cotisation du sympathisant : " + sympathizer.Name + " " + sympathizer.Surname;
            _context.Transactions.Add(transaction);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
