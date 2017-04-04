using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Stolons.ViewModels.Consumers;
using Microsoft.AspNetCore.Authorization;
using Stolons.Helpers;

namespace Stolons.Controllers
{
    public class UsersController : UsersBaseController
    {

        public UsersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(context,environment,userManager,signInManager,serviceProvider)
        {

        }

        // GET: Consumers
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_WedAdmin)]
        public IActionResult Index()
        {
            return View(_context.AdherentStolons.Include(x=>x.Stolon).Include(x=>x.Adherent).Where(x=>x.StolonId== GetCurrentStolon().Id));
        }        
    }
}
