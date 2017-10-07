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
using Stolons.ViewModels.Adherents;

namespace Stolons.Controllers
{
    public abstract class StolonsAdministrationController : BaseController
    {

        public StolonsAdministrationController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager,context,environment, signInManager)
        {

        }


        public abstract IActionResult Index();


        // GET: Stolons/Details/5
        public IActionResult DetailsStolon(Guid? id)
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
            return View(new StolonViewModel(GetActiveAdherentStolon(), stolon));
        }

        public PartialViewResult _PartialDetailsStolon(Guid? id)
        {
            var stolon = _context.Stolons.FirstOrDefault(m => m.Id == id);
            return PartialView(new StolonViewModel(GetActiveAdherentStolon(), stolon));

        }

        // GET: Stolons/Members
        public IActionResult Members(Guid? id)
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
            return View(new AdherentsViewModel(GetActiveAdherentStolon(),
                                                stolon,
                                                _context.GetSympathizers(stolon),
                                                _context.AdherentStolons.Include(x => x.Adherent).Where(x => x.StolonId == id).ToList()));
        }

        // GET: Stolons/Create
        public PartialViewResult _PartialCreateStolon()
        {
            return PartialView(new StolonViewModel(GetActiveAdherentStolon(), new Stolon()));
        }

        // POST: Stolons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStolon(StolonViewModel vm, IFormFile uploadFile)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            if (ModelState.IsValid)
            {
                vm.Stolon.Id = Guid.NewGuid();
                vm.Stolon.LogoFileName = await UploadFile(uploadFile, Configurations.StolonLogoStockagePath);
                vm.Stolon.CreationDate = DateTime.Now;
                _context.Add(vm.Stolon);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(vm);
        }


        // GET: Stolons/Edit/5
        public PartialViewResult _PartialEditStolon(Guid? id)
        {
            var stolon = _context.Stolons.SingleOrDefault(x => x.Id == id);

            return PartialView(new StolonViewModel(GetActiveAdherentStolon(), stolon));
        }

        // POST: Stolons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStolon(StolonViewModel vm, string uploadLogo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    switch (vm.Stolon.StolonType)
                    {
                        case Stolon.OrganisationType.Producer:
                            vm.Stolon.UseProducersFee = false;
                            vm.Stolon.UseSubscipstion = false;
                            vm.Stolon.UseSympathizer = false;
                            break;
                    }
                    if(!String.IsNullOrWhiteSpace(uploadLogo))
                    {
                        //Je supprime l'ancien logo
                        _environment.DeleteFile(StolonLogoStockagePath, vm.Stolon.LogoFileName);
                        //J'upload le nouveau et récupére son nom
                        vm.Stolon.LogoFileName = _environment.UploadBase64Image(uploadLogo, StolonLogoStockagePath);
                    }
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
                return RedirectToAction("Index");
            }
            return View(vm.Stolon);
        }

        public async Task<IActionResult> DeleteStolon(Guid id)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            var stolon = await _context.Stolons.SingleOrDefaultAsync(m => m.Id == id);
            _context.Stolons.Remove(stolon);
            await _context.SaveChangesAsync();
            _context.RemoveRange(_context.AdherentStolons.Where(x => x.StolonId == stolon.Id));
            return RedirectToAction("Index");
        }




        public IActionResult SwitchMode(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            Stolon stolon = _context.Stolons.FirstOrDefault(x => x.Id == id);
            stolon.SimulationMode = stolon.GetMode() == Stolon.Modes.DeliveryAndStockUpdate ? Stolon.Modes.Order : Stolon.Modes.DeliveryAndStockUpdate;
            _context.Update(stolon);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool StolonExists(Guid id)
        {
            return _context.Stolons.Any(e => e.Id == id);
        }
    }
}
