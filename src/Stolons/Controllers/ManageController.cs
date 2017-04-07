using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stolons.Models;
using Stolons.Services;
using Stolons.ViewModels.Manage;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Net.Http.Headers;
using Stolons.Models.Users;
using Stolons.ViewModels.Adherents;
using Microsoft.EntityFrameworkCore;

namespace Stolons.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {

        public ManageController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        { 

        }

        //
        // GET: /Manage/Index
        [HttpGet]
        [Authorize()]
        public IActionResult Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Votre mot de passe a été changé avec succès."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "Une erreur est survenue"
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";
            var adherentStolon = GetActiveAdherentStolon();

            return View(new ManageViewModel(adherentStolon, _context.AdherentStolons.Include(x=>x.Adherent).Include(x=>x.Stolon).Where(x=>x.AdherentId == adherentStolon.AdherentId).ToList()));
        }

        // GET: Consumers/Edit/5
        public IActionResult Edit()
        {
            AdherentStolon adherentStolon = GetActiveAdherentStolon();
            if (adherentStolon == null)
            {
                return NotFound();
            }
            bool isProducer = _context.AdherentStolons.Any(x => x.IsProducer && x.AdherentId == adherentStolon.AdherentId);
            return View(new AdherentViewModel(adherentStolon, adherentStolon.Adherent,isProducer? AdherentEdition.Producer: AdherentEdition.Adherent));
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);
                AdherentsBaseController.UpdateAdherent(_context, vmAdherent, uploadFile);
                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        [Authorize()]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentAppUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        [Authorize()]
        public async Task<IActionResult> ChangeUserInformations()
        {
            var user = await GetCurrentAppUserAsync();
            Adherent adherent = _context.Adherents.Single(x => x.Email == user.Email);
            if (adherent == null)
            {
                return NotFound();
            }
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return View(new AdherentViewModel());
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public IActionResult ChangeUserInformations(AdherentViewModel vmConsumer, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmConsumer.Adherent, uploadFile);
                ApplicationUser appUser = _context.Users.First(x => x.Email == vmConsumer.OriginalEmail);
                appUser.Email = vmConsumer.Adherent.Email;
                _context.Update(appUser);

                _context.Update(vmConsumer.Adherent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmConsumer);
        }

        [Authorize()]
        public async Task<IActionResult> ChangeProducerInformations()
        {
            var user = await GetCurrentAppUserAsync();
            Adherent producer = _context.Adherents.Single(m => m.Email == user.Email);
            if (producer == null)
            {
                return NotFound();
            }
            return View(new AdherentViewModel());
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public IActionResult ChangeProducerInformations(AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);
                 
                ApplicationUser appUser = _context.Users.First(x => x.Email == vmAdherent.OriginalEmail);
                appUser.Email = vmAdherent.Adherent.Email;
                _context.Update(appUser);
                _context.Update(vmAdherent.Adherent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }
        

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
        

        #endregion
    }
}
