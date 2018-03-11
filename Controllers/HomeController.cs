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
using Stolons.ViewModels.Chat;

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
                    vm.NewsVm = new ViewModels.News.NewsListViewModel(adherentStolon, _context.News.Include(x => x.PublishBy).ThenInclude(x => x.Adherent).Where(x => x.PublishBy.StolonId == adherentStolon.StolonId).Where(x => (x.PublishStart < DateTime.Now && x.PublishEnd > DateTime.Now) || (x.PublishEnd < DateTime.Now)).ToList(),true);
                else
                    vm.NewsVm = new ViewModels.News.NewsListViewModel(adherentStolon, _context.News.Include(x => x.PublishBy).ThenInclude(x=>x.Adherent).Where(x => x.PublishBy.StolonId == adherentStolon.StolonId).Where(x => x.PublishStart < DateTime.Now && x.PublishEnd > DateTime.Now).ToList());
                vm.ChatVm = new ChatMessageListViewModel(adherentStolon, _context.ChatMessages.Include(x=>x.PublishBy).Include(x=>x.PublishBy.Stolon).Include(x=>x.PublishBy.Adherent).Where(x => x.PublishBy.StolonId == adherentStolon.StolonId).OrderBy(x => x.DateOfPublication).Take(200).ToList());

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
        /// Contact and information about a Stolon
        /// </summary>
        /// <param name="id">Id of the Stolon</param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult StolonContact(Guid id)
        {
            Stolon stolon = _context.Stolons.FirstOrDefault(x => x.Id == id);
            Dictionary<AdherentStolon, List<Product>> prods = new Dictionary<AdherentStolon, List<Product>>();

            var producers = _context.AdherentStolons
            .Include(x => x.Adherent)
            .Include(x => x.Adherent.Products)
            .ThenInclude(x=>x.Familly)
            .ThenInclude(x=>x.Type)
            .Where(x => x.IsProducer && x.StolonId == stolon.Id)
            .AsNoTracking()
            .ToList();


            int totalProducts = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).Count(x => x.AdherentStolon.StolonId == stolon.Id && !x.Product.IsArchive);


            return View(new StolonContactViewModel(stolon, producers, totalProducts, User.Identity.IsAuthenticated?GetActiveAdherentStolon():null));
        }

        [Route("Go/{stolonName}")]

        [AllowAnonymous]
        public IActionResult GoToStolon(string stolonName)
        {
            if (!String.IsNullOrWhiteSpace(stolonName))
            {
                Stolon stolon = _context.Stolons.FirstOrDefault(x => x.Label == stolonName || x.ShortLabel == stolonName || x.Id.ToString() == stolonName);
                if (stolon != null)
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction(nameof(StolonContact), "Home", new { id = stolon.Id });
                    }
                    else
                    {
                        var adherentStolon = GetActiveAdherentStolon();
                        if (adherentStolon.StolonId != stolon.Id)
                            _context.AdherentStolons.FirstOrDefault(x => x.AdherentId == adherentStolon.AdherentId && x.StolonId == stolon.Id)?.SetHasActiveStolon(_context);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
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


        public IActionResult ChangeActiveStolon(Guid id)
        {
            GetActiveAdherentStolon().Adherent.AdherentStolons.First(x => x.StolonId == id).SetHasActiveStolon(_context);
            return RedirectToAction("Index");
        }
    }
}
