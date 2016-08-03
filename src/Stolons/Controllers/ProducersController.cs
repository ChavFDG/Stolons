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
    public class ProducersController : BaseController
    {
        private ApplicationDbContext _context;
        private IHostingEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProducersController(ApplicationDbContext context, IHostingEnvironment environment,
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
        // GET: Producer
        public IActionResult Index()
        {
            return View(_context.Producers.ToList());
        }

        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        // GET: Producer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Producer producer = _context.Producers.Single(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == producer.Email);
            return View(new ProducerViewModel(producer, await GetUserRole(producerAppUser)));
        }


        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        // GET: Producer/PartialDetails/5
        public async Task<IActionResult> PartialDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Producer producer = _context.Producers.Single(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == producer.Email);
            return PartialView(new ProducerViewModel(producer, await GetUserRole(producerAppUser)));
        }

        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        // GET: Producer/Create
        public IActionResult Create()
        {
            return View(new ProducerViewModel(new Producer(),Configurations.Role.User));
        }

        // POST: Producer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public async Task<IActionResult> Create(ProducerViewModel vmProducer, IFormFile uploadFile)
        {

            if (ModelState.IsValid)
            {
                #region Creating Producer
                string fileName = Configurations.DefaultFileName;
                if (uploadFile != null)
                {
                    //Image uploading
                    string uploads = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath);
                    fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
                    await uploadFile.SaveAsAsync(Path.Combine(uploads, fileName));
                }
                //Setting value for creation
                vmProducer.Producer.Avatar = Path.Combine(Configurations.UserAvatarStockagePath, fileName);
                vmProducer.Producer.RegistrationDate = DateTime.Now;
                _context.Producers.Add(vmProducer.Producer);
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
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Producer producer = _context.Producers.Single(m => m.Id == id);
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
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public async Task<IActionResult> Edit(ProducerViewModel vmProducer, IFormFile uploadFile, Configurations.Role UserRole)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    string uploads = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath);
                    //Deleting old image
                    string oldImage = Path.Combine(uploads, vmProducer.Producer.Avatar);
                    if (System.IO.File.Exists(oldImage) && vmProducer.Producer.Avatar != Path.Combine(Configurations.UserAvatarStockagePath, Configurations.DefaultFileName))
                        System.IO.File.Delete(Path.Combine(uploads, vmProducer.Producer.Avatar));
                    //Image uploading
                    string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
                    await uploadFile.SaveAsAsync(Path.Combine(uploads, fileName));
                    //Setting new value, saving
                    vmProducer.Producer.Avatar = Path.Combine(Configurations.UserAvatarStockagePath, fileName);
                }
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
        [Authorize(Roles = Configurations.Role_Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Producer producer = _context.Producers.Single(m => m.Id == id);
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
        [Authorize(Roles = Configurations.Role_Administrator)]
        public IActionResult DeleteConfirmed(int id)
        {
            Producer producer = _context.Producers.Single(m => m.Id == id);
            //Deleting image
            string uploads = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath);
            string image = Path.Combine(uploads, producer.Avatar);
            if (System.IO.File.Exists(image) && producer.Avatar != Path.Combine(Configurations.UserAvatarStockagePath, Configurations.DefaultFileName))
                System.IO.File.Delete(Path.Combine(uploads, producer.Avatar));
            //Delete App User
            ApplicationUser producerAppUser = _context.Users.First(x => x.Email == producer.Email);
            _context.Users.Remove(producerAppUser);
            //Delete User => TODO voir mieux car la on supprime tout ce qui est dépendant avant de supprimer le producteur, on fait le job de EF soit un Cascade delete !
            _context.News.RemoveRange(_context.News.Include(x => x.User).Where(x => x.User.Id == producer.Id));
            _context.Products.RemoveRange(_context.Products.Include(x => x.Producer).Where(x => x.Producer.Id == producer.Id));
            _context.ProducerBills.RemoveRange(_context.ProducerBills.Include(x => x.Producer).Where(x => x.Producer.Id == producer.Id));
            _context.Producers.Remove(producer);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private async Task<Configurations.Role> GetUserRole(ApplicationUser user)
        {

            IList<string> roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role);
        }
        private async Task<Configurations.Role> GetCurrentUserRole()
        {
            var user = await GetCurrentUserAsync(_userManager);
            return await GetUserRole(user);
        }
    }
}
