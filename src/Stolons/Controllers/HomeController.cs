using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Stolons.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Stolons.ViewModels.Home;
using Stolons.Models.Users;

namespace Stolons.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var user = GetCurrentStolonsUserSync();
            HomeViewModel vm = new HomeViewModel();
            if(user != null)
                vm.News = _context.News.Include(x => x.User).Where(x => x.StolonId == user.StolonId).ToList();
            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        /// <summary>
        /// Contact and infortion about a Stolon
        /// </summary>
        /// <param name="id">Id of the Stolon</param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult StolonContact(Guid id)
        {
            Stolon stolon = _context.Stolons.FirstOrDefault(x => x.Id == id);
            Dictionary<Producer, List<Product>> prods = new Dictionary<Producer, List<Product>>();

            foreach (var producer in _context.Producers.Include(x => x.Products))
            {
                prods.Add(producer, _context.Products.Include(x=>x.Familly).ThenInclude(x=>x.Type).Where(product => product.ProducerId == producer.Id).ToList());
            }
            return View(new StolonContactViewModel(stolon, prods));
        }

        [AllowAnonymous]
        public IActionResult Contact()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult HowItsWork()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
