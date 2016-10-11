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
        protected ApplicationDbContext _context;
        protected IHostingEnvironment _environment;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;

        public UsersBaseController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _environment = environment;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult PaySubscription(int id)
        {
            User user = _context.StolonsUsers.Single(m => m.Id == id);
            //
            user.Cotisation = true;
            //Add a transaction
            Transaction transaction = new Transaction();
            transaction.AddedAutomaticly = true;
            transaction.Date = DateTime.Now;
            transaction.Type = Transaction.TransactionType.Inbound;
            transaction.Category = Transaction.TransactionCategory.Subscription;
            transaction.Amount = Configurations.GetSubscriptionAmount(user);
            transaction.Description = "Paiement de la cotisation de : "+  user.Name + " " + user.Surname;
            _context.Transactions.Add(transaction);
            //Update
            _context.StolonsUsers.Update(user);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Enable(int id)
        {
            User user = _context.StolonsUsers.Single(m => m.Id == id);
            //
            user.DisableReason = "";
            user.Enable = true;
            //Update
            _context.StolonsUsers.Update(user);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Disable(int id, string comment)
        {
            User user = _context.StolonsUsers.Single(m => m.Id == id);
            //
            user.DisableReason = comment;
            user.Enable = false;
            //Update
            _context.StolonsUsers.Update(user);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
