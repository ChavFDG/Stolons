using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Stolons.Helpers;
using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Stolons.Configurations;

namespace Stolons.Controllers
{
    public class BaseController : Controller
    {
        IServiceProvider _serviceProvider;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly ApplicationDbContext _context;
        protected readonly IHostingEnvironment _environment;
        protected readonly SignInManager<ApplicationUser> _signInManager;

        public BaseController(IServiceProvider serviceProvider,
                                UserManager<ApplicationUser> userManager,
                                ApplicationDbContext dbContext,
                                IHostingEnvironment environment,
                                SignInManager<ApplicationUser> signInManager)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _context = dbContext;
            _signInManager = signInManager;
            _environment = environment;
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.ActionDescriptor.ActionName;
            }
            ViewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                var engine = _serviceProvider.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                // Resolver.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = engine.FindView(ControllerContext, viewName, false);
                ViewContext viewContext = new ViewContext(
                                      ControllerContext,
                                      viewResult.View,
                                      ViewData,
                                      TempData,
                                      sw,
                                      new HtmlHelperOptions() //Added this parameter in
                                      );
                //Everything is async now!
                var t = viewResult.View.RenderAsync(viewContext);
                t.Wait();
                return sw.GetStringBuilder().ToString();
            }
        }

        protected async Task<ApplicationUser> GetCurrentAppUserAsync()
        {
            return await _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User));
        }
        protected ApplicationUser GetCurrentAppUserSync()
        {
            return _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User)).GetAwaiter().GetResult();
        }

        protected async Task<StolonsUser> GetCurrentStolonsUserAsync()
        {
            ApplicationUser user = await _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User));
            return _context.StolonsUsers.Include(x => x.Stolon).FirstOrDefault(x => x.Email == user.Email);
        }
        protected StolonsUser GetCurrentStolonsUserSync()
        {
            ApplicationUser user = _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User)).GetAwaiter().GetResult();
            return _context.StolonsUsers.Include(x => x.Stolon).FirstOrDefault(x => x.Email == user.Email);
        }

        protected Stolon GetCurrentStolon()
        {
            return GetCurrentStolonsUserSync().Stolon;
        }


        /// <summary>
        /// Upload a file and return is name
        /// </summary>
        /// <param name="uploadFile">File to upload</param>
        /// <param name="stockagePath">Stockage path of the file</param>
        /// <param name="filePathToDelete">File path to delete</param>
        /// <returns></returns>
        protected async Task<string> UploadFile(IFormFile uploadFile, string stockagePath, string filePathToDelete = null)
        {
            if (!String.IsNullOrWhiteSpace(filePathToDelete) && !filePathToDelete.EndsWith(Configurations.DefaultFileName))
                if (System.IO.File.Exists(filePathToDelete))
                    System.IO.File.Delete(filePathToDelete);

            if (uploadFile == null)
                return String.IsNullOrWhiteSpace(filePathToDelete) || filePathToDelete.EndsWith(Configurations.DefaultFileName) ? null : Path.GetFileName(filePathToDelete);

            string uploads = Path.Combine(_environment.WebRootPath, stockagePath);
            string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');
            await uploadFile.SaveAsAsync(Path.Combine(uploads, fileName));
            return fileName;
        }
        protected async void UploadAndSetAvatar(Consumer consumer, IFormFile uploadFile)
        {
            consumer.AvatarFileName = await UploadFile(uploadFile, Configurations.AvatarStockagePath, consumer.AvatarFilePath);
        }
    }
}
