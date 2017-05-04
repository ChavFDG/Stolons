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
using Microsoft.AspNetCore.Authorization;
using Stolons.Helpers;
using Stolons.Models.Users;

namespace Stolons.Controllers
{
    public class UsersController : BaseController
    {

        public UsersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        // GET: Consumers
        public IActionResult Index()
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            Stolon stolon = GetCurrentStolon();
            List<IAdherent> iAdherents = new List<IAdherent>();
            _context.AdherentStolons.Include(x => x.Stolon).Include(x => x.Adherent).Where(x => x.StolonId == stolon.Id).ToList().ForEach(x => iAdherents.Add(x.Adherent));
            _context.Sympathizers.Where(x => x.StolonId == stolon.Id);
            return View(iAdherents); 
        }        
    }
}
