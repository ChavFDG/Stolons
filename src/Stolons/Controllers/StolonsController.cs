using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using static Stolons.Configurations;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Net.Http.Headers;
using Stolons.Helpers;
using Stolons.ViewModels.Stolons;

namespace Stolons.Controllers
{
    public class StolonsController : BaseController
    {
        public StolonsController(ApplicationDbContext context, IHostingEnvironment environment,
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        // GET: Stolons
        public IActionResult Index()
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            return View(new StolonsViewModel(GetActiveAdherentStolon(), _context.Stolons.ToList()));
        }

        // GET: Stolons/Details/5
        public IActionResult Details(Guid? id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var stolon = _context.Stolons.FirstOrDefault(m => m.Id == id);
            if (stolon == null)
            {
                return NotFound();
            }
            return View(new StolonViewModel(stolon, GetCurrentAdherentSync().IsWebAdmin));

        }

        // GET: Stolons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Stolons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StolonViewModel vm, IFormFile uploadFile)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            if (ModelState.IsValid)
            {
                vm.Stolon.Id = Guid.NewGuid();
                vm.Stolon.LogoFileName = await UploadFile(uploadFile, Configurations.StolonLogoStockagePath);
                _context.Add(vm.Stolon);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(vm);
        }


        // GET: Stolons/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var stolon = _context.Stolons.SingleOrDefault(x => x.Id == id);
            if (stolon == null)
            {
                return NotFound();
            }
            return View(new StolonViewModel(stolon,GetCurrentAdherentSync().IsWebAdmin));
        }

        // POST: Stolons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, StolonViewModel vm, IFormFile uploadFile)
        {
            if (id != vm.Stolon.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vm.Stolon.LogoFileName = await UploadFile(uploadFile, Configurations.AvatarStockagePath, vm.Stolon.LogoFileName);
                    _context.Update(vm.Stolon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StolonExists(vm.Stolon.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = id });
            }
            return View(vm.Stolon);
        }

        // GET: Stolons/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var stolon = _context.Stolons.FirstOrDefault(m => m.Id == id);
            if (stolon == null)
            {
                return NotFound();
            }
            return View(new StolonViewModel( stolon, GetCurrentAdherentSync().IsWebAdmin));
        }

        // POST: Stolons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            var stolon = await _context.Stolons.SingleOrDefaultAsync(m => m.Id == id);
            _context.Stolons.Remove(stolon);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }




        [Route("Stolons/SwitchMode")]
        public IActionResult SwitchMode(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            Stolon stolon = _context.Stolons.FirstOrDefault(x => x.Id == id);
            stolon.SimulationMode = stolon.GetMode() == Stolon.Modes.DeliveryAndStockUpdate ? Stolon.Modes.Order : Stolon.Modes.DeliveryAndStockUpdate;
            _context.Update(stolon);
            _context.SaveChanges();
            return RedirectToAction("Details", new { id = id });
        }

        private bool StolonExists(Guid id)
        {
            return _context.Stolons.Any(e => e.Id == id);
        }
    }
}
