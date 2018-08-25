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
using Stolons.ViewModels.Transactions;

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
        [Authorize()]
        public IActionResult Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            Stolon stolon = GetCurrentStolon();
            List<Transaction> transactions = _context.Transactions.Include(x => x.Stolon).Where(x => x.StolonId == stolon.Id).ToList();
            transactions.OfType<AdherentTransaction>().ToList().ForEach(transac => transac.Adherent = _context.Adherents.FirstOrDefault(adherent => adherent.Id == transac.AdherentId));
            return View(new TransactionsViewModel(GetActiveAdherentStolon(), stolon, transactions));
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

            return View(new TransactionViewModel(GetActiveAdherentStolon(),GetCurrentStolon(), transaction));
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(new TransactionViewModel(GetActiveAdherentStolon(), GetCurrentStolon(), new AdherentTransaction()));
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( TransactionViewModel transactionVm)
        {
            if (ModelState.IsValid)
            {
                AdherentTransaction adherentTransaction = new AdherentTransaction(  GetActiveAdherentStolon().Adherent,
                                                                                    GetCurrentStolon(),
                                                                                    transactionVm.Transaction.Type,
                                                                                    transactionVm.Transaction.Category,
                                                                                    transactionVm.Transaction.Amount,
                                                                                    transactionVm.Transaction.Description,
                                                                                    false);
                _context.Add(adherentTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(transactionVm);
        }

        // GET: Transactions/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var transaction = _context.Transactions.FirstOrDefault(x => x.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(new TransactionViewModel(GetActiveAdherentStolon(), GetCurrentStolon(), transaction));
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TransactionViewModel transactionVm)
        {
            if (id != transactionVm.Transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionVm.Transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transactionVm.Transaction.Id))
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
            return View(transactionVm);
        }

        // GET: Transactions/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            var transaction = _context.Transactions.FirstOrDefault(x => x.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(new TransactionViewModel(GetActiveAdherentStolon(), GetCurrentStolon(), transaction));
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
