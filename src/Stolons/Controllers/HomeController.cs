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
using static Stolons.Configurations;
using Stolons.Models.Messages;

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
        public IActionResult Index(bool showOldNews = false)
        {
            if (User.Identity.IsAuthenticated)
            {
                var adherentStolon = GetActiveAdherentStolon();
                HomeViewModel vm = new HomeViewModel(adherentStolon);
                if (showOldNews)
                    vm.NewsVm = new ViewModels.News.NewsListViewModel(adherentStolon, _context.News.Include(x => x.PublishBy).Where(x => x.PublishBy.StolonId == adherentStolon.StolonId).Where(x => (x.PublishStart < DateTime.Now && x.PublishEnd > DateTime.Now) || (x.PublishEnd < DateTime.Now)).ToList(),true);
                else
                    vm.NewsVm = new ViewModels.News.NewsListViewModel(adherentStolon, _context.News.Include(x => x.PublishBy).Where(x => x.PublishBy.StolonId == adherentStolon.StolonId).Where(x => x.PublishStart < DateTime.Now && x.PublishEnd > DateTime.Now).ToList());
                return View(vm);
            }
            else
                return View();
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
            Dictionary<Adherent, List<Product>> prods = new Dictionary<Adherent, List<Product>>();

            foreach (var producer in _context.Adherents.Include(x => x.Products))
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

        [Route("Home/ShowAllNews")]
        public IActionResult ShowAllNews()
        {
            if (!Authorized(Role.Adherent))
                return Unauthorized();

            return RedirectToAction("Index", new { showOldNews = true });
        }
    }
}
