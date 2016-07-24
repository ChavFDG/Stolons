using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Stolons.Helpers;

namespace Stolons.Controllers
{
    public class NewsController : BaseController
    {
        private ApplicationDbContext _context;
        private IHostingEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public NewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment environment,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;    
        }



        // GET: News
        public IActionResult Index()
        {
            return View(_context.News.Include(m => m.User).ToList());
        }

        // GET: News/Details/5
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News news = _context.News.Include(m => m.User).Single(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // GET: News/Create
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
        public async Task<IActionResult> Create(News news, IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                string fileName = Configurations.DefaultFileName;
                if (uploadFile != null)
                {
                    //Image uploading
                    string uploads = Path.Combine(_environment.WebRootPath, Configurations.NewsImageStockagePath);
                    fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
                    await uploadFile.SaveAsAsync(Path.Combine(uploads, fileName));
                }
                //Setting value for creation
                news.Id = Guid.NewGuid();
                news.DateOfPublication = DateTime.Now;
                news.ImageLink = Path.Combine(Configurations.NewsImageStockagePath,fileName);
                //TODO Get logged in User and add it to the news
                var appUser = await GetCurrentUserAsync(_userManager);
                User user;
                user = _context.Consumers.FirstOrDefault(x => x.Email == appUser.Email);
                if(user == null)
                {
                    user = _context.Producers.FirstOrDefault(x => x.Email == appUser.Email);
                }
                news.User = user;
                _context.News.Add(news);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(news);
        }

        // GET: News/Edit/5
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News news = _context.News.Include(m => m.User).Single(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
        public async Task<IActionResult> Edit(News news,IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    string uploads = Path.Combine(_environment.WebRootPath, Configurations.NewsImageStockagePath);
                    //Deleting old image
                    string oldImage = Path.Combine(uploads, news.ImageLink);
                    if (System.IO.File.Exists(oldImage) && news.ImageLink != Path.Combine(Configurations.NewsImageStockagePath,Configurations.DefaultFileName))
                        System.IO.File.Delete(Path.Combine(uploads, news.ImageLink));
                    //Image uploading
                    string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
                    await uploadFile.SaveAsAsync(Path.Combine(uploads, fileName));
                    //Setting new value, saving
                    news.ImageLink = Path.Combine(Configurations.NewsImageStockagePath, fileName);
                }
                var appUser = await GetCurrentUserAsync(_userManager);
                User user;
                user = _context.Consumers.FirstOrDefault(x => x.Email == appUser.Email);
		if (user == null)
                {
                    user = _context.Producers.FirstOrDefault(x => x.Email == appUser.Email);
                }
                news.User = user;
                _context.Update(news);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(news);
        }

        // GET: News/Delete/5
        [ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_Administrator)]
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News news = _context.News.Include(m => m.User).Single(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_Administrator)]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            News news = _context.News.Single(m => m.Id == id);
            //Deleting image
            string uploads = Path.Combine(_environment.WebRootPath, Configurations.NewsImageStockagePath);
            string image = Path.Combine(uploads, news.ImageLink);
            if (System.IO.File.Exists(image) && news.ImageLink != Path.Combine(Configurations.NewsImageStockagePath, Configurations.DefaultFileName))
                System.IO.File.Delete(Path.Combine(uploads, news.ImageLink));
            _context.News.Remove(news);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
