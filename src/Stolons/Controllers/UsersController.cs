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
    public class UsersController : BaseController
    {
        private ApplicationDbContext _context;
        private IHostingEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _environment = environment;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Consumers
        [Authorize(Roles = Configurations.Role_Volunteer + "," + Configurations.Role_Administrator)]
        public IActionResult Index()
        {
            return View(_context.StolonsUsers.ToList());
        }        
    }
}
