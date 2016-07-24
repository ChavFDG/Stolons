using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Controllers
{
    public class BaseController : Controller
    {
        IServiceProvider _serviceProvider;

        public BaseController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

        protected async Task<ApplicationUser> GetCurrentUserAsync(UserManager<ApplicationUser> userManager)
        {
            return await userManager.FindByIdAsync(userManager.GetUserId(HttpContext.User));
        }
        protected ApplicationUser GetCurrentUserSync(UserManager<ApplicationUser> userManager)
        {
            return userManager.FindByIdAsync(userManager.GetUserId(HttpContext.User)).GetAwaiter().GetResult();
        }
    }
}
