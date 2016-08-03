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
using Stolons.ViewModels.Sympathizers;
using Stolons.Models.Users;

namespace Stolons.Controllers
{
    public class SympathizersController : UsersBaseController
    {

        public SympathizersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context,environment,userManager,signInManager,serviceProvider)
        {

        }

        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        // GET: Sympathizer
        public IActionResult Index()
        {
            return View(_context.Sympathizers.ToList());
        }

        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        // GET: Sympathizer/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sympathizer sympathizer = _context.Sympathizers.Single(m => m.Id == id);
            if (sympathizer == null)
            {
                return NotFound();
            }
            return View(new SympathizerViewModel(sympathizer));
        }


        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        // GET: Sympathizer/PartialDetails/5
        public IActionResult PartialDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sympathizer Sympathizer = _context.Sympathizers.Single(m => m.Id == id);
            if (Sympathizer == null)
            {
                return NotFound();
            }
            return PartialView(new SympathizerViewModel(Sympathizer));
        }

        // GET: Sympathizer/Create
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Create()
        {
            return View(new SympathizerViewModel(new Sympathizer()));
        }

        // POST: Sympathizer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Create(SympathizerViewModel vmSympathizer)
        {
            if (ModelState.IsValid)
            {
                #region Creating Sympathizer
                string fileName = Configurations.DefaultFileName;

                //Setting value for creation
                vmSympathizer.Sympathizer.RegistrationDate = DateTime.Now;
                _context.Sympathizers.Add(vmSympathizer.Sympathizer);
                #endregion Creating Sympathizer

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vmSympathizer);
        }

        // GET: Sympathizer/Edit/5
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sympathizer sympathizer = _context.Sympathizers.Single(m => m.Id == id);
            if (sympathizer == null)
            {
                return NotFound();
            }

            return View(new SympathizerViewModel(sympathizer));
        }

        // POST: Sympathizer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Edit(SympathizerViewModel vmSympathizer)
        {
            if (ModelState.IsValid)
            {        
                _context.Update(vmSympathizer.Sympathizer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmSympathizer);
        }

        // GET: Sympathizer/Delete/5
        [ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_Administrator)]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Sympathizer sympathizer = _context.Sympathizers.Single(m => m.Id == id);
            if (sympathizer == null)
            {
                return NotFound();
            }
            return View(new SympathizerViewModel(sympathizer));
        }

        // POST: Sympathizer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Administrator)]
        public IActionResult DeleteConfirmed(int id)
        {
            Sympathizer sympathizer = _context.Sympathizers.Single(m => m.Id == id);           
            _context.Sympathizers.Remove(sympathizer);
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
