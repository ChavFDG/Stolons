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
using Stolons.ViewModels.Consumers;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Net.Http.Headers;
using Stolons.ViewModels.Producers;
using Stolons.Models.Users;

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
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Votre mot de passe a été changé avec succès."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "Une erreur est survenue"
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var user = await GetCurrentAppUserAsync();
            StolonsUser stolonsUser = _context.Consumers.FirstOrDefault(m => m.Email == user.Email);
            if (stolonsUser == null)
            {
                //It's a producer
                stolonsUser = _context.Producers.FirstOrDefault(m => m.Email == user.Email);
            }

            var model = new IndexViewModel
            {
                Avatar = stolonsUser.Avatar,
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
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
            Consumer consumer = _context.Consumers.Single(m => m.Email == user.Email);
            if (consumer == null)
            {
                return NotFound();
            }
            IList<string> roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new ConsumerViewModel(consumer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public async Task<IActionResult> ChangeUserInformations(ConsumerViewModel userStolonVm, IFormFile uploadFile, Configurations.Role UserRole)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    //Deleting old image
                    string oldImage = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath, userStolonVm.Consumer.Avatar);
                    if (System.IO.File.Exists(oldImage) && userStolonVm.Consumer.Avatar != Path.Combine(Configurations.UserAvatarStockagePath, Configurations.DefaultFileName))
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath, userStolonVm.Consumer.Avatar));
                    //Image uploading
                    string fileName = await Configurations.UploadImageFile(_environment, uploadFile, Configurations.UserAvatarStockagePath);
                    //Setting new value, saving
                    userStolonVm.Consumer.Avatar =  fileName;
                }
                ApplicationUser appUser = _context.Users.First(x => x.Email == userStolonVm.OriginalEmail);
                appUser.Email = userStolonVm.Consumer.Email;
                _context.Update(appUser);
                //Getting actual roles
                IList<string> roles = await _userManager.GetRolesAsync(appUser);
                if (!roles.Contains(UserRole.ToString()))
                {
                    string roleToRemove = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
                    await _userManager.RemoveFromRoleAsync(appUser, roleToRemove);
                    //Add user role
                    await _userManager.AddToRoleAsync(appUser, UserRole.ToString());
                }
                _context.Update(userStolonVm.Consumer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userStolonVm);
        }

        [Authorize()]
        public async Task<IActionResult> ChangeProducerInformations()
        {
            var user = await GetCurrentAppUserAsync();
            Producer producer = _context.Producers.Single(m => m.Email == user.Email);
            if (producer == null)
            {
                return NotFound();
            }
            IList<string> roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new ProducerViewModel(producer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize()]
        public async Task<IActionResult> ChangeProducerInformations(ProducerViewModel producerVm, IFormFile uploadFile, Configurations.Role UserRole)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    //Deleting old image
                    string oldImage = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath, producerVm.Producer.Avatar);
                    if (System.IO.File.Exists(oldImage) && producerVm.Producer.Avatar != Path.Combine(Configurations.UserAvatarStockagePath, Configurations.DefaultFileName))
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath, producerVm.Producer.Avatar));
                    //Image uploading
                    string fileName = await Configurations.UploadImageFile(_environment, uploadFile, Configurations.UserAvatarStockagePath);
                    //Setting new value, saving
                    producerVm.Producer.Avatar = Path.Combine( fileName);
                }
                ApplicationUser appUser = _context.Users.First(x => x.Email == producerVm.OriginalEmail);
                appUser.Email = producerVm.Producer.Email;
                _context.Update(appUser);
                //Getting actual roles
                IList<string> roles = await _userManager.GetRolesAsync(appUser);
                if (!roles.Contains(UserRole.ToString()))
                {
                    string roleToRemove = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
                    await _userManager.RemoveFromRoleAsync(appUser, roleToRemove);
                    //Add user role
                    await _userManager.AddToRoleAsync(appUser, UserRole.ToString());
                }
                _context.Update(producerVm.Producer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(producerVm);
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
