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
using Stolons.Models.Users;
using Stolons.Models.Messages;

namespace Stolons.Controllers
{
    public class NewsController : BaseController
    {

        public NewsController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }



        // GET: News
        public IActionResult Index()
        {
            return View(_context.News.Include(x => x.PublishBy).ThenInclude(x => x.Adherent).Include(x => x.PublishBy).ThenInclude(x => x.Stolon).Where(x=>x.PublishBy.StolonId == GetCurrentStolon().Id).ToList());
        }

        // GET: News/Details/5
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News news = _context.News.Include(x => x.PublishBy).ThenInclude(x => x.Adherent).Include(x => x.PublishBy).ThenInclude(x => x.Stolon).Single(x => x.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // GET: News/Create
        [Authorize(Roles = Configurations.Role_WedAdmin + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
        public IActionResult Create()
        {
            return View(new News(GetActiveAdherentStolon()));
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_WedAdmin + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
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
                    await uploadFile.SaveImageAsAsync(Path.Combine(uploads, fileName));
                    news.ImageLink = Path.Combine(Configurations.NewsImageStockagePath, fileName);
                }
                //Setting value for creation
                news.Id = Guid.NewGuid();
                news.DateOfPublication = DateTime.Now;
                //TODO Get logged in User and add it to the news
                news.PublishBy = GetActiveAdherentStolon();
                _context.News.Add(news);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(news);
        }

        // GET: News/Edit/5
        [Authorize(Roles = Configurations.Role_WedAdmin + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News news = _context.News.Include(m => m.PublishBy).ThenInclude(x=>x.Adherent).Include(x=>x.PublishBy).ThenInclude(x=>x.Stolon).Single(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Configurations.Role_WedAdmin + "," + Configurations.Role_Volunteer + "," +Configurations.UserType_Producer)]
        public async Task<IActionResult> Edit(News news,IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    string uploads = Path.Combine(_environment.WebRootPath, Configurations.NewsImageStockagePath);
                    //Deleting old image
                    DeleteImage(news.ImageLink);
                    //Image uploading
                    string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
                    await uploadFile.SaveImageAsAsync(Path.Combine(uploads, fileName));
                    //Setting new value, saving
                    news.ImageLink = Path.Combine(Configurations.NewsImageStockagePath, fileName);
                }
                news.PublishBy = GetActiveAdherentStolon();
                _context.Update(news);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(news);
        }

        // GET: News/Delete/5
        [ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_WedAdmin)]
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            News news = _context.News.Include(m => m.PublishBy).ThenInclude(x => x.Adherent).Include(x => x.PublishBy).ThenInclude(x => x.Stolon).Single(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = Configurations.Role_WedAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            News news = _context.News.Single(m => m.Id == id);
            DeleteImage(news.ImageLink);
            _context.News.Remove(news);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private void DeleteImage(string imagePath)
        {
            //Deleting image
            string image = Path.Combine(_environment.WebRootPath, imagePath);
            string defaultImage = Path.Combine(Configurations.NewsImageStockagePath, Configurations.DefaultFileName);
            if (System.IO.File.Exists(image) && imagePath != defaultImage)
                System.IO.File.Delete(image);
        }
    }
}
