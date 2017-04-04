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
using Stolons.ViewModels.Producers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Stolons.Helpers;
using Stolons.Models.Users;

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

        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        // GET: Producer
        public IActionResult Index()
        {
            return View(_context.Adherents.Include(x=>x.AdherentStolons).Where(x => x.AdherentStolons.Any(prodStolon=>prodStolon.StolonId == GetCurrentStolon().Id)).ToList());
        }

        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        // GET: Producer/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent producer = _context.Adherents.Single(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == producer.Email);
            return View(new ProducerViewModel(producer, await GetUserRole(producerAppUser)));
        }


        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        // GET: Producer/PartialDetails/5
        public async Task<IActionResult> PartialDetails(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent producer = _context.Adherents.Single(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == producer.Email);
            return PartialView(new ProducerViewModel(producer, await GetUserRole(producerAppUser)));
        }

        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        // GET: Producer/Create
        public IActionResult Create()
        {
            return View(new ProducerViewModel(new Adherent(),Configurations.Role.User));
        }

        // POST: Producer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Create(ProducerViewModel vmProducer, IFormFile uploadFile)
        {

            if (ModelState.IsValid)
            {
                #region Creating Producer
                UploadAndSetAvatar(vmProducer.Producer, uploadFile);
                AdherentStolon activeAdhrentStolon= GetActiveAdherentStolonOf(vmProducer.Producer);
                activeAdhrentStolon.RegistrationDate = DateTime.Now;
                activeAdhrentStolon.StolonId = GetCurrentStolon().Id;
                _context.Adherents.Add(vmProducer.Producer);
                #endregion Creating Producer

                #region Creating linked application data
                var producerAppUser = new ApplicationUser { UserName = vmProducer.Producer.Email, Email = vmProducer.Producer.Email };
                producerAppUser.User = vmProducer.Producer;

                var result = await _userManager.CreateAsync(producerAppUser, vmProducer.Producer.Email);
                if (result.Succeeded)
                {
                    //Add user role
                    result = await _userManager.AddToRoleAsync(producerAppUser, vmProducer.UserRole.ToString());
                    //Add user type
                    result = await _userManager.AddToRoleAsync(producerAppUser, Configurations.UserType.Producer.ToString());
                }
                #endregion Creating linked application data


                _context.SaveChanges();
                //Send confirmation mail
                Services.AuthMessageSender.SendEmail(vmProducer.Producer.Email, vmProducer.Producer.Name, "Creation de votre compte", base.RenderPartialViewToString("ProducerCreatedConfirmationMail", vmProducer));
                
                return RedirectToAction("Index");
            }

            return View(vmProducer);
        }

        // GET: Producer/Edit/5
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent producer = _context.Adherents.Single(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == producer.Email);

            return View(new ProducerViewModel(producer, await GetUserRole(producerAppUser)));
        }

        // POST: Producer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Edit(ProducerViewModel vmProducer, IFormFile uploadFile, Configurations.Role UserRole)
        {
            if (ModelState.IsValid)
            {
                UploadAndSetAvatar(vmProducer.Producer,uploadFile);
                ApplicationUser producerAppUser = _context.Users.First(x => x.Email == vmProducer.OriginalEmail);
                producerAppUser.Email = vmProducer.Producer.Email;
                _context.Update(producerAppUser);
                //Getting actual roles
                IList<string> roles = await _userManager.GetRolesAsync(producerAppUser);
                if (!roles.Contains(UserRole.ToString()))
                {
                    string roleToRemove = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
                    await _userManager.RemoveFromRoleAsync(producerAppUser, roleToRemove);
                    //Add user role
                    await _userManager.AddToRoleAsync(producerAppUser, UserRole.ToString());
                }
                _context.Update(vmProducer.Producer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmProducer);
        }

        // GET: Producer/Delete/5
        [ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_WedAdmin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Adherent producer = _context.Adherents.Single(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == producer.Email);
            return View(new ProducerViewModel(producer, await GetUserRole(producerAppUser)));
        }

        // POST: Producer/Delete/5
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
