using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
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

        [Authorize()]
        public IActionResult Index(MailMessage mailMessage)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (mailMessage == null)
                mailMessage = new MailMessage();
            return View(mailMessage);
        }

        // GET: News/Details/5
        public IActionResult Preview(MailMessage mailMessage)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            return View(mailMessage);
        }
        
        public IActionResult SendToSympathizers(MailMessage mailMessage)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();
            var users = _context.Sympathizers.AsNoTracking().Where(x => x.StolonId == GetCurrentStolon().Id && x.ReceivedInformationsEmail).ToList();
            var stolon = GetActiveAdherentStolon().Stolon;
            Task.Run(() =>
            {
                SendMail(users, mailMessage, stolon);
            });
            MailsSendedVm mailsSendedVm = new MailsSendedVm(users.ToList<IAdherent>(), mailMessage);
            return View("Report", mailsSendedVm);
        }
        
        public IActionResult SendToConsumers(MailMessage mailMessage)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();
            var users = _context.Adherents.AsNoTracking().Include(x => x.AdherentStolons).Where(x => x.AdherentStolons.Any(adhStol => adhStol.StolonId == GetCurrentStolon().Id && !adhStol.Deleted && x.ReceivedInformationsEmail)).ToList();
            var stolon = GetActiveAdherentStolon().Stolon;
            Task.Run(() =>
            {
                SendMail(users, mailMessage, stolon);
            });
            MailsSendedVm mailsSendedVm = new MailsSendedVm(users.ToList<IAdherent>(), mailMessage);
            return View("Report", mailsSendedVm);
        }
        
        public IActionResult SendToProducers(MailMessage mailMessage)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();
            var users = _context.Adherents.AsNoTracking().Include(x => x.AdherentStolons).Where(x => x.AdherentStolons.Any(adhStol => adhStol.StolonId == GetCurrentStolon().Id && !adhStol.Deleted && adhStol.IsProducer)).ToList();
            var stolon = GetActiveAdherentStolon().Stolon;
            Task.Run(() =>
            {
                SendMail(users, mailMessage,stolon);
            });
            MailsSendedVm mailsSendedVm = new MailsSendedVm(users.ToList<IAdherent>(), mailMessage);
            return View("Report", mailsSendedVm);

        }

        // GET: News/Details/5
        public IActionResult SendToAllUser(MailMessage mailMessage,Stolon stolon)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            List<IAdherent> users = new List<IAdherent>();
            users.AddRange(_context.Sympathizers.AsNoTracking().Where(x=>x.StolonId == stolon.Id && x.ReceivedInformationsEmail).ToList());
            users.AddRange(_context.Adherents.AsNoTracking().Include(x=>x.AdherentStolons).Where(x=>x.AdherentStolons.Any(adhStol=>adhStol.StolonId == stolon.Id && !adhStol.Deleted && x.ReceivedInformationsEmail)).ToList());

            Task.Run(() =>
            {
                SendMail(users, mailMessage, stolon);
            });
            MailsSendedVm mailsSendedVm = new MailsSendedVm(users, mailMessage);
            return View("Report", mailsSendedVm);
        }

        private void  SendMail(IEnumerable<IAdherent> users, MailMessage mailMessage, Stolon from)
        {
            foreach (IAdherent user in users)
            {
                if (!String.IsNullOrWhiteSpace(user.Email))
                { 
                    try
                    {
                        AuthMessageSender.SendEmail(from.Label,
                                                    user.Email,
                                                    user.Name,
                                                    mailMessage.Title,
                                                    mailMessage.Message);
                    }
                    catch(Exception ex)
                    {
                        DotnetHelper.GetLogger<MailsController>().LogError(ex.ToString());
                    }
                }
            }
        }
    }
}
