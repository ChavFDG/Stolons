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
using Stolons.ViewModels.News;

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
            return View(new NewsListViewModel(GetActiveAdherentStolon(), _context.News.Include(x => x.PublishBy).ThenInclude(x => x.Adherent).Include(x => x.PublishBy).ThenInclude(x => x.Stolon).Where(x => x.PublishBy.StolonId == GetCurrentStolon().Id).ToList()));
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
        public IActionResult Create()
        {
            if (Authorized(Role.Volunteer) || AuthorizedProducer())
                return View(new NewsViewModel(GetActiveAdherentStolon(), new News(GetActiveAdherentStolon())));
            return Unauthorized();

        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NewsViewModel newsVm, string uploadNewsImage)
        {
            if (Authorized(Role.Volunteer) || AuthorizedProducer())
            {
                if (ModelState.IsValid)
                {
                    string fileName = Configurations.DefaultImageFileName;
                    if (!String.IsNullOrWhiteSpace(uploadNewsImage))
                        newsVm.News.ImageName = _environment.UploadBase64Image(uploadNewsImage, Configurations.NewsImageStockagePath);
                    //Setting value for creation
                    newsVm.News.Id = Guid.NewGuid();
                    newsVm.News.DateOfPublication = DateTime.Now;
                    //TODO Get logged in User and add it to the news
                    newsVm.News.PublishBy = GetActiveAdherentStolon();
                    _context.News.Add(newsVm.News);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return Unauthorized();
        }

        // GET: News/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (Authorized(Role.Volunteer) || AuthorizedProducer())
            {
                if (id == null)
                {
                    return NotFound();
                }

                News news = _context.News.Include(m => m.PublishBy).ThenInclude(x => x.Adherent).Include(x => x.PublishBy).ThenInclude(x => x.Stolon).FirstOrDefault(x => x.Id == id);
                if (news == null)
                {
                    return NotFound();
                }
                return View(new NewsViewModel(GetActiveAdherentStolon(), news));

            }
            return Unauthorized();
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewsViewModel newsVm, string uploadNewsImage)
        {
            if (Authorized(Role.Volunteer) || AuthorizedProducer())
            {
                if (ModelState.IsValid)
                {

                    if (!String.IsNullOrWhiteSpace(uploadNewsImage))
                    {
                        _environment.DeleteFile(newsVm.News.ImageLink);
                        newsVm.News.ImageName = _environment.UploadBase64Image(uploadNewsImage, Configurations.NewsImageStockagePath);
                    }
                    newsVm.News.PublishBy = GetActiveAdherentStolon();
                    _context.Update(newsVm.News);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(newsVm);

            }
            return Unauthorized();
        }

        // GET: News/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(Guid? id)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            News news = _context.News.Include(m => m.PublishBy).ThenInclude(x => x.Adherent).Include(x => x.PublishBy).ThenInclude(x => x.Stolon).FirstOrDefault(x => x.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            News news = _context.News.FirstOrDefault(x => x.Id == id);
            _environment.DeleteFile(news.ImageLink);
            _context.News.Remove(news);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
