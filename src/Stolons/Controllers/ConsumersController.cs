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
using Stolons.ViewModels.Consumers;
using Microsoft.AspNetCore.Authorization;
using Stolons.Helpers;
using Stolons.Models.Users;
using Stolons.ViewModels.Token;

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
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult Index()
        {
            return View(_context.AdherentStolons.Include(x=>x.Adherent).Where(x => x.StolonId == GetCurrentStolon().Id).ToList());
        }
        
        // GET: Consumers/Details/5
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent consumer = _context.Adherents.Single(m => m.Id == id);
            if (consumer == null)
            {
                return NotFound();
            }
            ApplicationUser appUser = _context.Users.First(x => x.Email == consumer.Email);
            IList<string> roles = await _userManager.GetRolesAsync(appUser);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new ConsumerViewModel(consumer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // GET: Consumers/Create
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult Create()
        {
            return View(new ConsumerViewModel(new Adherent(),Configurations.Role.User));
        }

        // POST: Consumers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Create(ConsumerViewModel vmConsumer, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                #region Creating Consumer
                //Setting value for creation
                UploadAndSetAvatar(vmConsumer.Consumer, uploadFile);

                vmConsumer.Consumer.Name = vmConsumer.Consumer.Name.ToUpper();
                AdherentStolon consumerStolon = new AdherentStolon(vmConsumer.Consumer, GetCurrentStolon(), true);
                consumerStolon.RegistrationDate = DateTime.Now;
                _context.Adherents.Add(vmConsumer.Consumer);
                _context.AdherentStolons.Add(consumerStolon);
                #endregion Creating Consumer

                #region Creating linked application data
                var appUser = new ApplicationUser { UserName = vmConsumer.Consumer.Email, Email = vmConsumer.Consumer.Email };
                appUser.User = vmConsumer.Consumer;
                
                var result = await _userManager.CreateAsync(appUser, vmConsumer.Consumer.Email);
                if (result.Succeeded)
                {
                    //Add user role
                    result = await _userManager.AddToRoleAsync(appUser, vmConsumer.UserRole.ToString());
                    //Add user type
                    result = await _userManager.AddToRoleAsync(appUser, Configurations.UserType.Consumer.ToString());
                }
                #endregion Creating linked application data
                
                _context.SaveChanges();
                //Send confirmation mail
                Services.AuthMessageSender.SendEmail(vmConsumer.Consumer.Email, vmConsumer.Consumer.Name, "Creation de votre compte", base.RenderPartialViewToString("UserCreatedConfirmationMail", vmConsumer));

                return RedirectToAction("Index");
            }
            return View(vmConsumer);
        }

        // GET: Consumers/Edit/5
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent consumer = _context.Adherents.Single(m => m.Id == id);
            if (consumer == null)
            {
                return NotFound();
            }
            ApplicationUser appUser = _context.Users.First(x => x.Email == consumer.Email);
            IList<string> roles = await _userManager.GetRolesAsync(appUser);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new ConsumerViewModel(consumer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Edit(ConsumerViewModel vmConsumer, IFormFile uploadFile, Configurations.Role UserRole)
        {
            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmConsumer.Consumer, uploadFile);
                ApplicationUser appUser = _context.Users.First(x => x.Email == vmConsumer.OriginalEmail);
                appUser.Email = vmConsumer.Consumer.Email;
                vmConsumer.Consumer.Name = vmConsumer.Consumer.Name.ToUpper();
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
                _context.Update(vmConsumer.Consumer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmConsumer);
        }

        // GET: Consumers/Delete/5
        [ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent consumer = _context.Adherents.Single(m => m.Id == id);
            if (consumer == null)
            {
                return NotFound();
            }
            ApplicationUser appUser = _context.Users.First(x => x.Email == consumer.Email);
            IList<string> roles = await _userManager.GetRolesAsync(appUser);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new ConsumerViewModel(consumer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // POST: Consumers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_WedAdmin)]
        public IActionResult DeleteConfirmed(Guid id)
        {
            Adherent adherent = _context.Adherents.Single(m => m.Id == id);
            //Deleting image
            string uploads = Path.Combine(_environment.WebRootPath, Configurations.AvatarStockagePath);
            string image = Path.Combine(uploads, adherent.AvatarFileName);
            if (System.IO.File.Exists(image) && adherent.AvatarFileName != Path.Combine(Configurations.AvatarStockagePath, Configurations.DefaultFileName))
                System.IO.File.Delete(Path.Combine(uploads, adherent.AvatarFileName));
            //Delete App User
            ApplicationUser appUser = _context.Users.First(x => x.Email == adherent.Email);
            _context.Users.Remove(appUser);
            //Delete User
            //TODO ajouter les bill entry

            _context.News.RemoveRange(_context.News.Include(x => x.PublishBy).ThenInclude(x=>x.Adherent).Where(x => x.PublishBy.Adherent.Id == adherent.Id));
            _context.TempsWeekBaskets.RemoveRange(_context.TempsWeekBaskets.Include(x => x.Consumer).Where(x => x.Consumer.Id == adherent.Id));
            _context.ValidatedWeekBaskets.RemoveRange(_context.ValidatedWeekBaskets.Include(x => x.Consumer).Where(x => x.Consumer.Id == adherent.Id));
            _context.ConsumerBills.RemoveRange(_context.ConsumerBills.Include(x => x.Adherent).Where(x => x.Adherent.Id == adherent.Id));
            _context.Adherents.Remove(adherent);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Consumers/CreditToken/5
        [ActionName("CreditToken")]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult CreditToken(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent consumer = _context.Adherents.Single(m => m.Id == id);
            if (consumer == null)
            {
                return NotFound();
            }

            return View(new CreditTokenViewModel(consumer));
        }

        // POST: Consumers/CreditToken
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult CreditToken(CreditTokenViewModel vmCreditToken)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x=>x.Adherent).Single(x => x.AdherentId == vmCreditToken.Consumer.Id);
            adherentStolon.Token += vmCreditToken.CreditedToken;
            _context.Add(new Transaction(
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
