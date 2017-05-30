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
    public class ConsumersController : AdherentsBaseController
    {

        public ConsumersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context,environment,userManager,signInManager,serviceProvider)
        {

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
                "Créditage du compte de "+ adherentStolon.Adherent.Name + "( " + adherentStolon.LocalId + " ) de "  + vmCreditToken.CreditedToken + "??"));
            _context.Update(adherentStolon);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }   
}
