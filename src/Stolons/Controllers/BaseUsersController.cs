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
        /// <summary>
        /// PaySympathiserSubscription
        /// </summary>
        /// <param name="id">Sympathizer ID</param>
        /// <returns></returns>
        public IActionResult PaySympathiserSubscription(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            Transaction transaction = new Transaction();
            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            Stolon currentStolon = GetCurrentStolon();
            sympathizer.SubscriptionPaid = true;
            transaction.Amount = currentStolon.SympathizerSubscription;
            //Add a transaction
            transaction.Stolon = currentStolon;
            transaction.AddedAutomaticly = true;
            transaction.Date = DateTime.Now;
            transaction.Type = Transaction.TransactionType.Inbound;
            transaction.Category = Transaction.TransactionCategory.Subscription;
            transaction.Description = "Paiement de la cotisation du sympathisant : " + sympathizer.Name + " " + sympathizer.Surname;
            _context.Transactions.Add(transaction);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
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
