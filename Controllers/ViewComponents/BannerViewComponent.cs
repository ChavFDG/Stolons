using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Stolons.ViewModels.Banner;
using Stolons.Models;
using Stolons.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Stolons.Controllers
{
    public class BannerViewComponent : ViewComponent
    {
        private ApplicationDbContext _dbContext;
        
        private readonly UserManager<ApplicationUser> _userManager;
        

        public BannerViewComponent(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                ApplicationUser appUser = await _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User));
                AdherentStolon adherentStolon = _dbContext.AdherentStolons.Include(x => x.Adherent).ThenInclude(x=>x.AdherentStolons).Include(x => x.Stolon).FirstOrDefault(x => x.IsActiveStolon && x.Adherent.Email.Equals(appUser.Email, StringComparison.CurrentCultureIgnoreCase));
                adherentStolon.Adherent.AdherentStolons.ForEach(x => x.Stolon = _dbContext.Stolons.FirstOrDefault(stolon => stolon.Id == x.StolonId));
                TempWeekBasket tempWeekBasket = _dbContext.TempsWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).FirstOrDefault(x => x.AdherentStolon.Id == adherentStolon.Id);
                if(tempWeekBasket != null)
                    tempWeekBasket.Validated = WeekBasketController.IsBasketValidated(tempWeekBasket, _dbContext);
                ConsumerBill consumerBill = _dbContext.ConsumerBills.FirstOrDefault(x => x.AdherentStolon.Id == adherentStolon.Id && x.State == BillState.Pending);


                return View(new BannerViewModel(adherentStolon, tempWeekBasket, consumerBill));
            }
            else
            {
                return View();
            }
        }

    }
}
