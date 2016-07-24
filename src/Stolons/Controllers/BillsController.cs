using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Stolons.Controllers
{
    public class BillsController : BaseController
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IHostingEnvironment _environment;

        public BillsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment environment,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
        }

        // GET: Bills
        [Authorize()]
        public async Task<IActionResult> Index()
        {
            var appUser = await GetCurrentUserAsync(_userManager);
            var stolonsUser = _context.StolonsUsers.First(x => x.Email == appUser.Email);
            if(stolonsUser is Producer)
            {
                return View(_context.ProducerBills.Where(x=>x.Producer.Email == stolonsUser.Email).OrderBy(x=>x.EditionDate).ToList<IBill>());
            }
            else if (stolonsUser is Consumer)
            {
                return View(_context.ConsumerBills.Where(x => x.Consumer.Email == stolonsUser.Email).OrderBy(x => x.EditionDate).ToList<IBill>());
            }
            return View();//ERROR
        }
    }
}
