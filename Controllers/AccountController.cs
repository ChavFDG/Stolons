using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stolons.Models;
using Stolons.Services;
using Stolons.ViewModels.Account;
using Stolons.ViewModels.Manage;
using Stolons.Models.Users;
using Microsoft.AspNetCore.Hosting;

namespace Stolons.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {

        public AccountController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //D'abord on regarde si il existe bien un User avec ce mail
                Adherent stolonsUser = _context.Adherents.Include(x=>x.AdherentStolons).FirstOrDefault(x => model.Email.Equals(x.Email, StringComparison.CurrentCultureIgnoreCase));
                if (stolonsUser == null)
                {
                    ModelState.AddModelError("LoginFailed", "Utilisateur inconnu");
                    return View(model);
                }
                else
                {
                    AdherentStolon activeAdherentStolon = stolonsUser.AdherentStolons.First(x => x.IsActiveStolon);
                    //On regarde si le compte de l'utilisateur est actif
                    if (!activeAdherentStolon.Enable)
                    {
                        ModelState.AddModelError("LoginFailed", "Votre compte a été bloqué pour la raison suivante : " + activeAdherentStolon.DisableReason);
                        return View(model);
                    }
                    // Il a un mot de passe, on le log si il est bon
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                    if (result.IsLockedOut)
                    {
                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError("LoginFailed", "Erreur dans la saisie du mot de passe");
                        return View(model);
                    }
                }
                
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

	[HttpGet]
        [AllowAnonymous]
	public IActionResult ForgotPassword()
	{
	    return View();
	}

	// POST /Account/ForgotPassword
	[HttpPost]
        [AllowAnonymous]
	public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
	    if (!ModelState.IsValid)
	    {
		return View(model);
	    }
	    Adherent stolonsUser = _context.Adherents.FirstOrDefault(x => model.Email.Equals(x.Email, StringComparison.CurrentCultureIgnoreCase));
	    if (stolonsUser == null)
	    {
		return View("ForgotPasswordConfirmation");
	    }
	    ApplicationUser appUser = await _userManager.FindByEmailAsync(stolonsUser.Email.ToString());
	    string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
	    resetPasswordToken = System.Net.WebUtility.UrlEncode(resetPasswordToken);
	    string link = "http://" + Configurations.SiteUrl + "/Account/ResetPassword?token=" + resetPasswordToken + "&mail=" + stolonsUser.Email;

	    //Send mail
	    ForgotPasswordEmailViewModel vmodel = new ForgotPasswordEmailViewModel(stolonsUser, link);
	    AuthMessageSender.SendEmail(Configurations.Application.StolonsLabel, stolonsUser.Email, "", "Stolons: Oubli de votre mot de passe", base.RenderPartialViewToString("ResetPasswordEmailTemplate", vmodel), null, null);
	    return View("ForgotPasswordConfirmation");
	}

	[HttpGet]
        [AllowAnonymous]
	public IActionResult ResetPassword([FromQuery] string token, [FromQuery] string mail)
	{
	    var model = new ResetPasswordViewModel(token, mail);
	    return View("ResetPassword", model);
	}

	[HttpPost]
        [AllowAnonymous]
	public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
	    Adherent stolonsUser = _context.Adherents.FirstOrDefault(x => model.Email.Equals(x.Email, StringComparison.CurrentCultureIgnoreCase));
	    if (stolonsUser == null)
	    {
		return View(model);
	    }
	    ApplicationUser appUser = await _userManager.FindByEmailAsync(stolonsUser.Email);
	    var result = await _userManager.ResetPasswordAsync(appUser, model.Token, model.Password);
	    if (result.Succeeded)
	    {
		return View("ResetPasswordSuccess");
	    }
	    return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string userId, string code)
        {
            return View();
        }

	#region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}

        // // GET: /Account/ConfirmEmail
        // [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> ConfirmEmail(string userId, string code)
        // {
        //     if (userId == null || code == null)
        //     {
        //         return View("Error");
        //     }
        //     var user = await _userManager.FindByIdAsync(userId);
        //     if (user == null)
        //     {
        //         return View("Error");
        //     }
        //     var result = await _userManager.ConfirmEmailAsync(user, code);
        //     return View(result.Succeeded ? "ConfirmEmail" : "Error");
        // }

