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

namespace Stolons.Controllers
{
    public abstract class UsersBaseController : BaseController
    {

        public UsersBaseController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }
        
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult PaySubscription(Guid id)
        {
            Adherent user = _context.Adherents.Include(x=>x.ActiveAdherentStolon).Single(m => m.Id == id);
            //
            user.ActiveAdherentStolon.SubscriptionPaid = true;
            //Add a transaction
            Transaction transaction = new Transaction();
            transaction.AddedAutomaticly = true;
            transaction.Date = DateTime.Now;
            transaction.Type = Transaction.TransactionType.Inbound;
            transaction.Category = Transaction.TransactionCategory.Subscription;
            transaction.Amount = user.ActiveAdherentStolon.GetSubscriptionAmount();
            transaction.Description = "Paiement de la cotisation de : "+  user.Name + " " + user.Surname;
            _context.Transactions.Add(transaction);
            //Update
            _context.Adherents.Update(user);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult Enable(Guid id)
        {
            Adherent user = _context.Adherents.Include(x => x.ActiveAdherentStolon).Single(m => m.Id == id);
            //
            user.ActiveAdherentStolon.DisableReason = "";
            user.ActiveAdherentStolon.Enable = true;
            //Update
            _context.Adherents.Update(user);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult Disable(Guid id, string comment)
        {
            Adherent user = _context.Adherents.Include(x => x.ActiveAdherentStolon).Single(m => m.Id == id);
            //
            user.ActiveAdherentStolon.DisableReason = comment;
            user.ActiveAdherentStolon.Enable = false;
            //Update
            _context.Adherents.Update(user);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Return User Role of the specified application user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected async Task<Configurations.Role> GetUserRole(ApplicationUser user)
        {

            IList<string> roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role);
        }

        /// <summary>
        /// Return the role of the current application user
        /// </summary>
        /// <returns></returns>
        protected async Task<Configurations.Role> GetCurrentUserRole()
        {
            var user = await GetCurrentAppUserAsync();
            return await GetUserRole(user);
        }
    }
}
