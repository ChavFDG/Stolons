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
using System.Security.Claims;
using Stolons.Helpers;
using Stolons.Models.Users;
using Stolons.ViewModels.Adherents;

namespace Stolons.Controllers
{
    public class ProducersController : AdherentsBaseController
    {
        public override AdherentEdition EditionType
        {
            get
            {
                return AdherentEdition.Producer;
            }
        }

        public ProducersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context,environment,userManager,signInManager,serviceProvider)
        {

        }
        
        // GET: Producer
        public override IActionResult Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new AdherentsStolonViewModel(GetActiveAdherentStolon(), _context.AdherentStolons.Include(x=>x.Adherent).Include(x=>x.Stolon).Where(x => x.IsProducer).ToList()));
        }
        
               
        // GET: Producer/PartialDetails/5
        public IActionResult PartialDetails(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            Adherent adherent = _context.Adherents.FirstOrDefault(x => x.Id == id);
            if (adherent == null)
            {
                return NotFound();
            }
            return PartialView(new AdherentViewModel(GetActiveAdherentStolon(), adherent, AdherentEdition.Producer));
        }

        // POST: Producer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public override IActionResult DeleteConfirmed(Guid id)
        {

            if (!Authorized(Role.Admin))
                return Unauthorized();
            //Faut voir comment on fait, on vire tout les produits, on les désactive tous et on garde une sortie d'historique ?
            //Le mieux c'est pour une suppression final de cacher le producteur comme pour les vielles news

            return RedirectToAction("Index");
        }
    }
}
