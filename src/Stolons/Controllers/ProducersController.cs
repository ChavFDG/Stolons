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
    public class ProducersController : UsersBaseController
    {

        public ProducersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context,environment,userManager,signInManager,serviceProvider)
        {

        }
        
        // GET: Producer
        public IActionResult Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(_context.Adherents.Include(x=>x.AdherentStolons).Where(x => x.AdherentStolons.Any(prodStolon=>prodStolon.StolonId == GetCurrentStolon().Id)).ToList());
        }
        
        // GET: Producer/Details/5
        public IActionResult Details(Guid id)
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
            return View(new AdherentViewModel(GetActiveAdherentStolon(), adherent,AdherentEdition.Producer));
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
        
        // GET: Producer/Create
        public IActionResult Create()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new AdherentViewModel(GetActiveAdherentStolon(),new Adherent(), AdherentEdition.Producer));
        }

        // POST: Producer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();


            if (ModelState.IsValid)
            {
                #region Creating Producer
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);
                AdherentStolon activeAdhrentStolon= GetActiveAdherentStolonOf(vmAdherent.Adherent);
                activeAdhrentStolon.RegistrationDate = DateTime.Now;
                activeAdhrentStolon.StolonId = GetCurrentStolon().Id;
                _context.Adherents.Add(vmAdherent.Adherent);
                #endregion Creating Producer

                #region Creating linked application data
                var producerAppUser = new ApplicationUser { UserName = vmAdherent.Adherent.Email, Email = vmAdherent.Adherent.Email };
                producerAppUser.User = vmAdherent.Adherent;

                var result = await _userManager.CreateAsync(producerAppUser, vmAdherent.Adherent.Email);
                #endregion Creating linked application data


                _context.SaveChanges();
                //Send confirmation mail
                Services.AuthMessageSender.SendEmail(vmAdherent.Adherent.Email, vmAdherent.Adherent.Name, "Creation de votre compte", base.RenderPartialViewToString("ProducerCreatedConfirmationMail", vmAdherent));
                
                return RedirectToAction("Index");
            }

            return View(vmAdherent);
        }

        // GET: Producer/Edit/5
        public IActionResult Edit(Guid? id)
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

            return View(new AdherentViewModel(GetActiveAdherentStolon(),adherent, AdherentEdition.Producer));
        }

        // POST: Producer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AdherentViewModel vmAdherent, IFormFile uploadFile, Role role)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmAdherent.Adherent,uploadFile);
                ApplicationUser producerAppUser = _context.Users.First(x => x.Email == vmAdherent.OriginalEmail);
                producerAppUser.Email = vmAdherent.Adherent.Email;
                _context.Update(producerAppUser);
                _context.Update(vmAdherent.Adherent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }

        // GET: Producer/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(Guid id)
        {
            if (!Authorized(Role.Admin))
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
            return View(new AdherentViewModel(GetActiveAdherentStolon(), adherent, AdherentEdition.Producer));
        }

        // POST: Producer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            Adherent adherent = _context.Adherents.FirstOrDefault(x => x.Id == id);
            //Deleting image
            string uploads = Path.Combine(_environment.WebRootPath, Configurations.AvatarStockagePath);
            string image = Path.Combine(uploads, adherent.AvatarFileName);
            if (System.IO.File.Exists(image) && adherent.AvatarFileName != Path.Combine(Configurations.AvatarStockagePath, Configurations.DefaultFileName))
                System.IO.File.Delete(Path.Combine(uploads, adherent.AvatarFileName));
            //Delete App User
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == adherent.Email);
            _context.Users.Remove(producerAppUser);
            //Delete User => TODO voir mieux car la on supprime tout ce qui est dépendant avant de supprimer le producteur, on fait le job de EF soit un Cascade delete !
            _context.News.RemoveRange(_context.News.Include(x => x.PublishBy).Where(x => x.PublishBy.Adherent.Id == adherent.Id));
            _context.Products.RemoveRange(_context.Products.Include(x => x.Producer).Where(x => x.Producer.Id == adherent.Id));
            _context.ProducerBills.RemoveRange(_context.ProducerBills.Include(x => x.Adherent).Where(x => x.Adherent.Id == adherent.Id));
            _context.Adherents.Remove(adherent);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
