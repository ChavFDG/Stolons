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
        [Authorize(Roles = Role_StolonAdmin + "," + Role_WedAdmin)]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Stolons.ToListAsync());
        }

        // GET: Stolons/Details/5
        [Authorize(Roles = Role_StolonAdmin + "," + Role_WedAdmin)]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stolon = await _context.Stolons
                .SingleOrDefaultAsync(m => m.Id == id);
            if (stolon == null)
            {
                return NotFound();
            }

            return View(stolon);
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
        [Authorize(Roles = Role_WedAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Label,LogoFileName,Address,PhoneNumber,ContactMailAddress,UseProducersFee,ProducersFee,AboutText,JoinUsText,UseSubscipstion,UseSympathizer,SympathizerSubscription,ConsumerSubscription,ProducerSubscription,SubscriptionStartMonth,OrderDeliveryMessage,OrderDayStartDate,OrderHourStartDate,OrderMinuteStartDate,DeliveryAndStockUpdateDayStartDate,DeliveryAndStockUpdateDayStartDateHourStartDate,DeliveryAndStockUpdateDayStartDateMinuteStartDate,BasketPickUpStartDay,BasketPickUpStartHour,BasketPickUpStartMinute,BasketPickEndUpDay,BasketPickUpEndHour,BasketPickUpEndMinute,IsModeSimulated,SimulationMode,State,StolonStateMessage,Latitude,Longitude,GoodPlan")] Stolon stolon)
        {
            if (ModelState.IsValid)
            {
                stolon.Id = Guid.NewGuid();
                _context.Add(stolon);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(stolon);
        }

        // GET: Stolons/Edit/5
        [Authorize(Roles = Role_StolonAdmin + "," + Role_WedAdmin)]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stolon = await _context.Stolons.SingleOrDefaultAsync(m => m.Id == id);
            if (stolon == null)
            {
                return NotFound();
            }
            return View(stolon);
        }

        // POST: Stolons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role_StolonAdmin + "," + Role_WedAdmin)]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Label,LogoFileName,Address,PhoneNumber,ContactMailAddress,UseProducersFee,ProducersFee,AboutText,JoinUsText,UseSubscipstion,UseSympathizer,SympathizerSubscription,ConsumerSubscription,ProducerSubscription,SubscriptionStartMonth,OrderDeliveryMessage,OrderDayStartDate,OrderHourStartDate,OrderMinuteStartDate,DeliveryAndStockUpdateDayStartDate,DeliveryAndStockUpdateDayStartDateHourStartDate,DeliveryAndStockUpdateDayStartDateMinuteStartDate,BasketPickUpStartDay,BasketPickUpStartHour,BasketPickUpStartMinute,BasketPickEndUpDay,BasketPickUpEndHour,BasketPickUpEndMinute,IsModeSimulated,SimulationMode,State,StolonStateMessage,Latitude,Longitude,GoodPlan")] Stolon stolon)
        {
            if (id != stolon.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stolon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StolonExists(stolon.Id))
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
            return View(stolon);
        }

        // GET: Stolons/Delete/5
        [Authorize(Roles = Role_WedAdmin)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stolon = await _context.Stolons
                .SingleOrDefaultAsync(m => m.Id == id);
            if (stolon == null)
            {
                return NotFound();
            }

            return View(stolon);
        }

        // POST: Stolons/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = Role_WedAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var stolon = await _context.Stolons.SingleOrDefaultAsync(m => m.Id == id);
            _context.Stolons.Remove(stolon);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }



        [HttpPost, ActionName("SwitchMode")]
        [Authorize(Roles = Role_StolonAdmin + "," + Role_WedAdmin)]
        public IActionResult SwitchMode(Guid id)
        {
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
