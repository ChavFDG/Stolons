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
using Stolons.Helpers;
using Stolons.Models.Users;
using Stolons.Models.Transactions;
using Stolons.ViewModels.Adherents;

namespace Stolons.Controllers
{
    public abstract class AdherentsBaseController : BaseController
    {
        public AdherentsBaseController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        // GET: Consumers/DetailsAdherent/5
        public virtual IActionResult DetailsAdherent(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).FirstOrDefault(x => x.Id == id);
            if (adherentStolon == null)
            {
                return NotFound();
            }

            return View(new AdherentStolonViewModel(GetActiveAdherentStolon(), adherentStolon));
        }


        // GET: Consumers/Create
        public virtual IActionResult CreateAdherent(AdherentEdition edition,Guid? stolonId = null)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            Stolon stolon = stolonId == null ? GetCurrentStolon() : _context.Stolons.First(x => x.Id == stolonId);
            return View(new AdherentViewModel(GetActiveAdherentStolon(), new Adherent(), stolon, edition));
        }

        // POST: Consumers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(AdherentEdition edition,AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                Stolon stolon = _context.Stolons.FirstOrDefault(x => x.Id == vmAdherent.Stolon.Id);
                #region Creating Consumer
                //Setting value for creation
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);

                vmAdherent.Adherent.Name = vmAdherent.Adherent.Name.ToUpper();
                AdherentStolon adherentStolon = new AdherentStolon(vmAdherent.Adherent, stolon, true);
                adherentStolon.RegistrationDate = DateTime.Now;
                adherentStolon.LocalId = _context.AdherentStolons.Where(x => x.StolonId == stolon.Id).Max(x => x.LocalId) + 1;
                adherentStolon.IsProducer = edition == AdherentEdition.Producer;
                _context.Adherents.Add(vmAdherent.Adherent);
                _context.AdherentStolons.Add(adherentStolon);
                #endregion Creating Consumer

                #region Creating linked application data
                var appUser = new ApplicationUser { UserName = vmAdherent.Adherent.Email, Email = vmAdherent.Adherent.Email };
                appUser.User = vmAdherent.Adherent;

                var result = await _userManager.CreateAsync(appUser, vmAdherent.Adherent.Email);

                #endregion Creating linked application data
                //
                //
                _context.SaveChanges();
                //Send confirmation mail
                string confirmationViewName = edition == AdherentEdition.Consumer ? "AdherentConfirmationMail" : "ProducerConfirmationMail";
                Services.AuthMessageSender.SendEmail(vmAdherent.Adherent.Email, vmAdherent.Adherent.Name, "Creation de votre compte", base.RenderPartialViewToString(confirmationViewName, adherentStolon));

                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }
        
        public IActionResult EditAdherent(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).FirstOrDefault(x => x.Id == id);
            if (adherentStolon == null)
            {
                return NotFound();
            }
            return View(new AdherentViewModel(GetActiveAdherentStolon(), adherentStolon.Adherent, adherentStolon.Stolon, AdherentEdition.Consumer));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAdherent(AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);
                UpdateAdherent(_context, vmAdherent, uploadFile);
                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }

        public static void UpdateAdherent(ApplicationDbContext context, AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            ApplicationUser appUser = context.Users.First(x => x.Email == vmAdherent.OriginalEmail);
            appUser.Email = vmAdherent.Adherent.Email;
            vmAdherent.Adherent.Name = vmAdherent.Adherent.Name.ToUpper();
            context.Update(appUser);
            context.Update(vmAdherent.Adherent);
            context.SaveChanges();
        }

        // GET: Consumers/Delete/5
        [ActionName("Delete")]
        public IActionResult DeleteAdherent(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).FirstOrDefault(x => x.Id == id);
            if (adherentStolon == null)
            {
                return NotFound();
            }
            return View(new AdherentStolonViewModel(GetActiveAdherentStolon(), adherentStolon));
        }

        // POST: Consumers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public virtual IActionResult DeleteConfirmed(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);

            //Check if adherent is in an other Stolons
            if (_context.AdherentStolons.Any(x => x.AdherentId == adherentStolon.AdherentId && x.StolonId != adherentStolon.StolonId))
            {
            }
            //Delete the adherent from this stolons
            _context.AdherentStolons.Remove(adherentStolon);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Pay subscription
        /// </summary>
        /// <param name="id">AdherentStolon id</param>
        /// <returns></returns>
        public IActionResult PaySubscription(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();
            //Adherent stolon
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Stolon).Include(x => x.Adherent).First(x => x.Id == id);

            adherentStolon.SubscriptionPaid = true;
            _context.AdherentStolons.Update(adherentStolon);
            //Transaction
            Transaction transaction = new Transaction();
            transaction = new AdherentTransaction() { Adherent = adherentStolon.Adherent };
            //Update
            transaction.Amount = Configurations.GetSubscriptionAmount(adherentStolon);
            //Add a transaction
            transaction.Stolon = adherentStolon.Stolon;
            transaction.AddedAutomaticly = true;
            transaction.Date = DateTime.Now;
            transaction.Type = Transaction.TransactionType.Inbound;
            transaction.Category = Transaction.TransactionCategory.Subscription;
            transaction.Description = "Paiement de la cotisation de l'adhérant "+ (adherentStolon.IsProducer?"producteur":"")+" : "+ adherentStolon.Adherent.Name + " " + adherentStolon.Adherent.Surname;
            _context.Transactions.Add(transaction);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult EnableAdherent(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.First(x=>x.Id == id);

            adherentStolon.DisableReason = "";
            adherentStolon.Enable = true;
            //Update
            _context.AdherentStolons.Update(adherentStolon);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        
        public IActionResult DisableAdherent(Guid id, string comment)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);
            //
            adherentStolon.DisableReason = comment;
            adherentStolon.Enable = false;
            //Update
            _context.AdherentStolons.Update(adherentStolon);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
