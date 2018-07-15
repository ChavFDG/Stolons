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
using System.Collections.Generic;
using Newtonsoft.Json;
using Stolons.ViewModels.Chat;

namespace Stolons.Controllers
{
    public class ChatController : BaseController
    {

        public ChatController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        [HttpPost, ActionName("AddMessage")]
        public IActionResult AddMessage(string message, DateTime date)
        {
            var adherentStolon = GetActiveAdherentStolon();
            _context.ChatMessages.Add(new ChatMessage(adherentStolon, message));
            _context.SaveChanges();
            
            return GetNewMessages(date);
        }

        [HttpPost, ActionName("GetPreviousMessage")]
        public IActionResult GetPreviousMessages(DateTime date)
        {
            date = date.AddMilliseconds(1);
            var adherentStolon = GetActiveAdherentStolon();
            ChatMessageListViewModel chatMessagesViewModel 
                = new ChatMessageListViewModel(adherentStolon, 
                _context.ChatMessages.Include(x => x.PublishBy).Include(x => x.PublishBy.Stolon).Include(x => x.PublishBy.Adherent)
                    .Where(x => x.PublishBy.StolonId == adherentStolon.StolonId && x.DateOfPublication < date).OrderBy(x => x.DateOfPublication).ToList());
            return Json(chatMessagesViewModel);
        }

        [HttpPost, ActionName("GetNewMessages")]
        public IActionResult GetNewMessages(DateTime date)
        {
            date = date.AddMilliseconds(1);
            var adherentStolon = GetActiveAdherentStolon();
            ChatMessageListViewModel chatMessagesViewModel
                = new ChatMessageListViewModel(adherentStolon,
                _context.ChatMessages.Include(x => x.PublishBy).Include(x => x.PublishBy.Stolon).Include(x => x.PublishBy.Adherent)
                    .Where(x => x.PublishBy.StolonId == adherentStolon.StolonId && x.DateOfPublication > date).OrderBy(x => x.DateOfPublication).ToList());
            return Json(chatMessagesViewModel);
        }

        [HttpPost, ActionName("RemoveMessage")]
        public IActionResult RemoveMessage(Guid id)
        {
            var messsageToRemove = _context.ChatMessages.FirstOrDefault(x => x.Id == id);
            if (messsageToRemove == null || !Authorized(Role.Admin))
                return Json(false);
            _context.Remove(messsageToRemove);
            _context.SaveChanges();
            return Json(true);
        }
    }
}
