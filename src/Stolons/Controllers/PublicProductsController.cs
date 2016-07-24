using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Stolons.Controllers
{
    public class PublicProductsController : BaseController
    {
        private ApplicationDbContext _context;

        public PublicProductsController(ApplicationDbContext context,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _context = context;    
        }

        // GET: PublicProducts
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_context.Products.ToList());
        }        
    }
}
