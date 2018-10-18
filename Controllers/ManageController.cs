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
                : message == ManageMessageId.ChangeEmail ? "Un courriel vous a été envoyé pour confirmer le changement de courriel"
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
            return View(new AdherentViewModel(adherentStolon, adherentStolon.Adherent, adherentStolon.Stolon, isProducer? AdherentEdition.Producer: AdherentEdition.Consumer,false));
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
                    AuthMessageSender.SendEmail(Configurations.Application.StolonsLabel,
                                                    user.Email,
                                                    user.Email,
                                                    "Confirmation de changement de mot de passe",
                                                    "<h3>Confirmation du changement de mot de passe :</h3><p> Nouveau mot de passe : " + model.NewPassword + "</p>");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        // GET: /Manage/ChangeEmail
        [HttpGet]
        [Authorize()]
        public IActionResult ChangeEmail()
        {
            return View(new ChangeEmailViewModel() {UserId = GetCurrentAdherentSync().Id });
        }

        //
        // POST: /Manage/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var adherent = _context.Adherents.FirstOrDefault(x => x.Id == vm.UserId);
            if (adherent == null)
                return NotFound("Impossible de trouver l'utilisateur : " + vm.UserId);
            var appUser = await GetCurrentAppUserAsync();
            if (appUser != null)
            {
                var token = await _userManager.GenerateChangeEmailTokenAsync(appUser, vm.NewEmail);
                var confirmationLink = Url.Action("ChangeEmailToken", "Manage", new { token, oldEmail = adherent.Email, newEmail = vm.NewEmail }, protocol: HttpContext.Request.Scheme);
                AuthMessageSender.SendEmail(Configurations.Application.StolonsLabel, 
                    vm.NewEmail,
                    vm.NewEmail, 
                    "Confirmation de changement de courriel",
                    "<h2>Confirmation de changement de courriel</h2> " + "<a href=\"" + confirmationLink + "\">Cliquer ici pour valider votre changement de courriel</a>");

                return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangeEmail});                
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ChangeEmailToken([FromQuery] string token, [FromQuery] string oldEmail, [FromQuery] string newEmail)
        {
            //Change for appUser
            ApplicationUser appUser = _context.Users.First(x => x.Email == oldEmail);
            if (appUser == null)
                return NotFound("Impossible de trouver l'utilisateur : " + oldEmail);
            var result = await _userManager.ChangeEmailAsync(appUser, newEmail, token);
            if (result.Succeeded)
            {
                //Change mail for Adherent
                var adherent = _context.Adherents.FirstOrDefault(x => x.Email == oldEmail);
                if (adherent == null)
                    return NotFound("Impossible de trouver l'utilisateur : " + oldEmail);
                adherent.Email = newEmail;
                _context.SaveChanges();
            }
            
            await _signInManager.RefreshSignInAsync(appUser);
            await _signInManager.SignOutAsync();
            return Ok("Votre courriel vient d'être changé");
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
            ChangeEmail,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
        

        #endregion
    }
}
