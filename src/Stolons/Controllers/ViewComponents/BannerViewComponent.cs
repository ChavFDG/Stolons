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
                StolonsUser user = _dbContext.StolonsUsers.FirstOrDefault(x => x.Email.Equals(appUser.Email, StringComparison.CurrentCultureIgnoreCase));
                return View(new BannerViewModel(user));
            } else
            {
                return View();
            }
        }

    }
}
