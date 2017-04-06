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
using Stolons.ViewModels.Token;
using Stolons.Models.Transactions;
using Stolons.ViewModels.Adherents;

namespace Stolons.Controllers
{
    public class ConsumersController : UsersBaseController
    {

        public ConsumersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context,environment,userManager,signInManager,serviceProvider)
        {

        }

        // GET: Consumers
        public IActionResult Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new AdherentsStolonViewModel(GetActiveAdherentStolon(), _context.AdherentStolons.Include(x=>x.Adherent).Where(x => x.StolonId == GetCurrentStolon().Id).ToList()));
        }
        
        // GET: Consumers/Details/5
        public IActionResult Details(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x=>x.Adherent).Include(x=>x.Stolon).FirstOrDefault(x => x.Id == id);
            if (adherentStolon == null)
            {
                return NotFound();
            }

            return View(new AdherentStolonViewModel(adherentStolon) { ActiveAdherentStolon = GetActiveAdherentStolon() });
        }

        // GET: Consumers/Create
        public IActionResult Create()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new AdherentViewModel(GetActiveAdherentStolon(), new Adherent(),AdherentEdition.Adherent));
        }

        // POST: Consumers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdherentViewModel vmAdherent, IFormFile uploadFile)
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
                Services.AuthMessageSender.SendEmail(vmAdherent.Adherent.Email, vmAdherent.Adherent.Name, "Creation de votre compte", base.RenderPartialViewToString("UserCreatedConfirmationMail", adherentStolon));

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
            ApplicationUser appUser = _context.Users.First(x => x.Email == adherentStolon.Adherent.Email);
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
            return View(new AdherentStolonViewModel(adherentStolon));
        }

        // POST: Consumers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);

            //Check if adherent is in an other Stolons
            if(_context.AdherentStolons.Any(x=>x.AdherentId == adherentStolon.AdherentId && x.StolonId != adherentStolon.StolonId))
            {
            }
            //Delete the adherent from this stolons
            _context.AdherentStolons.Remove(adherentStolon);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Consumers/CreditToken/5
        [ActionName("CreditToken")]
        public IActionResult CreditToken(Guid id)
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

            return View(new CreditTokenViewModel(GetActiveAdherentStolon(), adherentStolon));
        }

        // POST: Consumers/CreditToken
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreditToken(CreditTokenViewModel vmCreditToken)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x=>x.Adherent).First(x => x.Id == vmCreditToken.AdherentStolon.Id);
            adherentStolon.Token += vmCreditToken.CreditedToken;
            _context.Add(new AdherentTransaction(
                adherentStolon.Adherent,
                adherentStolon.Stolon,
                Transaction.TransactionType.Inbound,
                Transaction.TransactionCategory.TokenCredit,
                vmCreditToken.CreditedToken,                
                "Créditage du compte de "+ adherentStolon.Adherent.Name + "( " + adherentStolon.Id + " ) de "  + vmCreditToken.CreditedToken + "??"));
            _context.Update(adherentStolon);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }   
}
