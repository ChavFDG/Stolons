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
using Stolons.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Stolons.Helpers;

namespace Stolons.Controllers
{
    public class UsersController : BaseController
    {
        private ApplicationDbContext _context;
        private IHostingEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _environment = environment;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Consumers
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Index()
        {
            return View(_context.Consumers.ToList());
        }

        // GET: Consumers/Details/5
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Consumer consumer = _context.Consumers.Single(m => m.Id == id);
            if (consumer == null)
            {
                return NotFound();
            }
            ApplicationUser appUser = _context.Users.First(x => x.Email == consumer.Email);
            IList<string> roles = await _userManager.GetRolesAsync(appUser);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new UserStolonViewModel(consumer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // GET: Consumers/Create
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Create()
        {
            return View(new UserStolonViewModel(new Consumer(),Configurations.Role.User));
        }

        // POST: Consumers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public async Task<IActionResult> Create(UserStolonViewModel vmConsumer, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                #region Creating Consumer
                string fileName = Configurations.DefaultFileName;
                if (uploadFile != null)
                {
                    //Image uploading
                    string uploads = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath);
                    fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
                    await uploadFile.SaveAsAsync(Path.Combine(uploads, fileName));
                }
                //Setting value for creation
                vmConsumer.Consumer.Avatar = Path.Combine(Configurations.UserAvatarStockagePath, fileName);
                vmConsumer.Consumer.RegistrationDate = DateTime.Now;
                vmConsumer.Consumer.Name = vmConsumer.Consumer.Name.ToUpper();
                _context.Consumers.Add(vmConsumer.Consumer);
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
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Consumer consumer = _context.Consumers.Single(m => m.Id == id);
            if (consumer == null)
            {
                return NotFound();
            }
            ApplicationUser appUser = _context.Users.First(x => x.Email == consumer.Email);
            IList<string> roles = await _userManager.GetRolesAsync(appUser);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new UserStolonViewModel(consumer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // POST: Consumers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public async Task<IActionResult> Edit(UserStolonViewModel consumerVm, IFormFile uploadFile, Configurations.Role UserRole)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    string uploads = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath);
                    //Deleting old image
                    string oldImage = Path.Combine(uploads, consumerVm.Consumer.Avatar);
                    if (System.IO.File.Exists(oldImage) && consumerVm.Consumer.Avatar != Path.Combine(Configurations.UserAvatarStockagePath, Configurations.DefaultFileName))
                        System.IO.File.Delete(Path.Combine(uploads, consumerVm.Consumer.Avatar));
                    //Image uploading
                    string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
                    await uploadFile.SaveAsAsync(Path.Combine(uploads, fileName));
                    //Setting new value, saving
                    consumerVm.Consumer.Avatar = Path.Combine(Configurations.UserAvatarStockagePath, fileName);
                }
                ApplicationUser appUser = _context.Users.First(x => x.Email == consumerVm.OriginalEmail);
                appUser.Email = consumerVm.Consumer.Email;
                consumerVm.Consumer.Name = consumerVm.Consumer.Name.ToUpper();
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
                _context.Update(consumerVm.Consumer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(consumerVm);
        }

        // GET: Consumers/Delete/5
        [ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Consumer consumer = _context.Consumers.Single(m => m.Id == id);
            if (consumer == null)
            {
                return NotFound();
            }
            ApplicationUser appUser = _context.Users.First(x => x.Email == consumer.Email);
            IList<string> roles = await _userManager.GetRolesAsync(appUser);
            string role = roles.FirstOrDefault(x => Configurations.GetRoles().Contains(x));
            return View(new UserStolonViewModel(consumer, (Configurations.Role)Enum.Parse(typeof(Configurations.Role), role)));
        }

        // POST: Consumers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Administrator)]
        public IActionResult DeleteConfirmed(int id)
        {
            Consumer consumer = _context.Consumers.Single(m => m.Id == id);
            //Deleting image
            string uploads = Path.Combine(_environment.WebRootPath, Configurations.UserAvatarStockagePath);
            string image = Path.Combine(uploads, consumer.Avatar);
            if (System.IO.File.Exists(image) && consumer.Avatar != Path.Combine(Configurations.UserAvatarStockagePath, Configurations.DefaultFileName))
                System.IO.File.Delete(Path.Combine(uploads, consumer.Avatar));
            //Delete App User
            ApplicationUser appUser = _context.Users.First(x => x.Email == consumer.Email);
            _context.Users.Remove(appUser);
            //Delete User
            //TODO ajouter les bill entry

            _context.News.RemoveRange(_context.News.Include(x => x.User).Where(x => x.User.Id == consumer.Id));
            _context.TempsWeekBaskets.RemoveRange(_context.TempsWeekBaskets.Include(x => x.Consumer).Where(x => x.Consumer.Id == consumer.Id));
            _context.ValidatedWeekBaskets.RemoveRange(_context.ValidatedWeekBaskets.Include(x => x.Consumer).Where(x => x.Consumer.Id == consumer.Id));
            _context.ConsumerBills.RemoveRange(_context.ConsumerBills.Include(x => x.Consumer).Where(x => x.Consumer.Id == consumer.Id));
            _context.Consumers.Remove(consumer);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
