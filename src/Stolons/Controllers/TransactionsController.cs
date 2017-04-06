using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Stolons.Models.Transactions;

namespace Stolons.Controllers
{
    public class TransactionsController : BaseController
    {


        public TransactionsController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {
               
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(await _context.Transactions.Include(x=>x.Stolon).Where(x=>x.StolonId == GetCurrentStolon().Id).ToListAsync());
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.SingleOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new Transaction());
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Category,Date,Description,Type")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.Id = Guid.NewGuid();
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.SingleOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Amount,Category,Date,Description,Type")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
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
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.SingleOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var transaction = await _context.Transactions.SingleOrDefaultAsync(m => m.Id == id);
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool TransactionExists(Guid id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
