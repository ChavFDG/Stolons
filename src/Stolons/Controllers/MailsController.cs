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
using System.Collections.Generic;
using Stolons.Services;
using Stolons.ViewModels.Mails;

namespace Stolons.Controllers
{
    public class MailsController : BaseController
    {


        public MailsController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {   
        }



        // GET: News
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer)]
        public IActionResult Index(MailMessage mailMessage)
        {
            if (mailMessage == null)
                mailMessage = new MailMessage();
            return View(mailMessage);
        }

        // GET: News/Details/5
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer )]
        public IActionResult Preview(MailMessage mailMessage)
        {
            return View(mailMessage);
        }

        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer)]
        public IActionResult SendToSympathizers(MailMessage mailMessage)
        {
            return View("Report", SendMail(_context.Consumers, mailMessage));
        }
        
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer )]
        public IActionResult SendToConsumers(MailMessage mailMessage)
        {
            return View("Report",SendMail(_context.Consumers, mailMessage));
        }
        
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer)]
        public IActionResult SendToProducers(MailMessage mailMessage)
        {
            return View("Report",SendMail(_context.Producers, mailMessage));
        }

        // GET: News/Details/5
        [Authorize(Roles = Configurations.Role_Administrator + "," + Configurations.Role_Volunteer)]
        public IActionResult SendToAllUser(MailMessage mailMessage)
        {
            List<StolonsUser> users = new List<StolonsUser>();
            users.AddRange(_context.Sympathizers);
            users.AddRange(_context.Consumers);
            users.AddRange(_context.Producers);
            return View("Report",  SendMail(users,mailMessage));
        }

        private MailsSendedReport SendMail(IEnumerable<StolonsUser> users, MailMessage mailMessage)
        {
            MailsSendedReport report = new MailsSendedReport();
            foreach (StolonsUser user in users)
            {
                if (!String.IsNullOrWhiteSpace(user.Email))
                { 
                    try
                    {
                        AuthMessageSender.SendEmail(user.Email,
                                                        user.Name,
                                                        mailMessage.Title,
                                                        mailMessage.Message);
                        report.MailsSended++;
                    }
                    catch(Exception except)
                    {
                        report.MailsNotSended++;
                    }
                }
            }
            return report;
        }
    }
}
