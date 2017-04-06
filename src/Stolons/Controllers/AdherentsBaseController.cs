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
        public abstract AdherentEdition EditionType { get;}

        public AdherentsBaseController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }
        // GET: Consumers
        public virtual IActionResult Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new AdherentsStolonViewModel(GetActiveAdherentStolon(), _context.AdherentStolons.Include(x => x.Adherent).Where(x => x.StolonId == GetCurrentStolon().Id).ToList()));
        }

        // GET: Consumers/Details/5
        public virtual IActionResult Details(Guid id)
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
        public virtual IActionResult Create()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new AdherentViewModel(GetActiveAdherentStolon(), new Adherent(), EditionType));
        }

        // POST: Consumers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                #region Creating Consumer
                //Setting value for creation
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);

                vmAdherent.Adherent.Name = vmAdherent.Adherent.Name.ToUpper();
                AdherentStolon adherentStolon = new AdherentStolon(vmAdherent.Adherent, GetCurrentStolon(), true);
                adherentStolon.RegistrationDate = DateTime.Now;
                adherentStolon.LocalId = _context.AdherentStolons.Where(x => x.StolonId == GetCurrentStolon().Id).Max(x => x.LocalId) + 1;
                adherentStolon.IsProducer = EditionType == AdherentEdition.Producer;
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
                Services.AuthMessageSender.SendEmail(vmAdherent.Adherent.Email, vmAdherent.Adherent.Name, "Creation de votre compte", base.RenderPartialViewToString("CreationConfirmationMail", adherentStolon));

                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }

        // GET: Consumers/Edit/5
        public IActionResult Edit(Guid id)
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
            return View(new AdherentViewModel(GetActiveAdherentStolon(), adherentStolon.Adherent, AdherentEdition.Adherent));
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AdherentViewModel vmAdherent, IFormFile uploadFile, Role role)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);
                ApplicationUser appUser = _context.Users.First(x => x.Email == vmAdherent.OriginalEmail);
                appUser.Email = vmAdherent.Adherent.Email;
                vmAdherent.Adherent.Name = vmAdherent.Adherent.Name.ToUpper();
                _context.Update(appUser);
                _context.Update(vmAdherent.Adherent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }

        // GET: Consumers/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(Guid id)
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
        
        public IActionResult Enable(Guid id)
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

        
        public IActionResult Disable(Guid id, string comment)
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
