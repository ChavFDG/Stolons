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
using Microsoft.AspNetCore.Authorization;
using Stolons.Helpers;
using Stolons.Models.Users;
using Stolons.ViewModels.Adherents;
using Stolons.Models.Transactions;
using Stolons.ViewModels.Token;
using Stolons.ViewModels.Sympathizers;

namespace Stolons.Controllers
{
    public class UsersController : BaseController
    {

        public UsersController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }


        [Authorize()]
        public IActionResult Index(Guid? id = null)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            Stolon stolon = id == null ? GetCurrentStolon() : _context.Stolons.First(x => x.Id == id);
            AdherentsViewModel adherentsViewModel = new AdherentsViewModel(GetActiveAdherentStolon(), stolon, _context.Sympathizers.Where(x => x.StolonId == stolon.Id).ToList(), _context.AdherentStolons.Include(x => x.Stolon).Include(x => x.Adherent).Where(x => x.StolonId == stolon.Id && !x.Deleted).ToList());
            return View(adherentsViewModel);
        }


        public virtual PartialViewResult _PartialDetailsAdherent(Guid id)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).FirstOrDefault(x => x.Id == id);
            return PartialView(new AdherentStolonViewModel(GetActiveAdherentStolon(), adherentStolon));
        }

        public virtual PartialViewResult _PartialAddAdherent(AdherentEdition edition, Guid? stolonId = null)
        {
            Stolon stolon = stolonId == null ? GetCurrentStolon() : _context.Stolons.First(x => x.Id == stolonId);
            List<string> emails;
            if (edition == AdherentEdition.Producer)
            {
                emails = _context.Adherents.Include(x => x.AdherentStolons).Where(x => x.AdherentStolons.Any(prod => prod.IsProducer)).Select(x => x.Email).ToList();
            }
            else
            {
                emails = _context.Adherents.Select(x => x.Email).ToList();
            }
            _context.AdherentStolons.Include(x => x.Adherent).Where(x => x.StolonId == stolon.Id && !x.Deleted).ToList().ForEach(x => emails.Remove(x.Adherent.Email));
            return PartialView(new SelectAdherentViewModel(GetActiveAdherentStolon(), stolon, emails, edition == AdherentEdition.Producer));
        }

        public virtual PartialViewResult _PartialEditProducerFee(Guid? adherentStolonId)
        {
            return PartialView(new ProducerFeeViewModel(GetActiveAdherentStolon(), _context.AdherentStolons.Include(x=>x.Adherent).First(x => x.Id == adherentStolonId)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual IActionResult EditProducerFee(ProducerFeeViewModel producerFeeViewModel)
        {

            if (!Authorized(Role.Admin))
                return Unauthorized();

            var producer = _context.AdherentStolons.First(x => x.Id == producerFeeViewModel.AdherentStolon.Id);
            producer.ProducerFee = producerFeeViewModel.ProducerFee;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual IActionResult AddAdherent(SelectAdherentViewModel selectAdherentViewModel)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                var adherent = _context.Adherents.First(x => x.Email == selectAdherentViewModel.SelectedEmail.Trim());
                var stolon = _context.Stolons.First(x => x.Id == selectAdherentViewModel.Stolon.Id);
                var oldAdherentStolon = _context.AdherentStolons.FirstOrDefault(x => x.AdherentId == adherent.Id && x.StolonId == stolon.Id);
                AdherentStolon adherentStolon = null;
                if (oldAdherentStolon == null)
                {
                    adherentStolon = new AdherentStolon(adherent, stolon);
                    adherentStolon.RegistrationDate = DateTime.Now;
                    if (_context.AdherentStolons.Where(x => x.StolonId == selectAdherentViewModel.Stolon.Id).Any())
                        adherentStolon.LocalId = _context.AdherentStolons.Where(x => x.StolonId == selectAdherentViewModel.Stolon.Id).Max(x => x.LocalId) + 1;
                    else
                        adherentStolon.LocalId = 1;
                    _context.AdherentStolons.Add(adherentStolon);
                }
                else
                {
                    adherentStolon = oldAdherentStolon;
                    adherentStolon.Deleted = false;
                }
                _context.SaveChanges();
                if (selectAdherentViewModel.AddHasProducer)
                {
                    SetAsProducer(adherentStolon.Id);
                }
                //Send confirmation mail
                Services.AuthMessageSender.SendEmail(adherentStolon.Stolon.Label, adherentStolon.Adherent.Email, adherentStolon.Adherent.Name, "Adhésion à un nouveau Stolon", base.RenderPartialViewToString("AdherentConfirmationMail", adherentStolon));

                return RedirectToAction("Index");
            }
            return View(selectAdherentViewModel);
        }

        public virtual PartialViewResult _PartialCreateAdherent(AdherentEdition edition, Guid? stolonId = null)
        {
            Stolon stolon = stolonId == null ? GetCurrentStolon() : _context.Stolons.First(x => x.Id == stolonId);
            return PartialView(new AdherentViewModel(GetActiveAdherentStolon(), new Adherent(), stolon, edition));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> CreateAdherent(AdherentEdition edition, AdherentViewModel vmAdherent, IFormFile uploadFile)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            
            if(_context.Adherents.Any(x=>x.Email == vmAdherent.Adherent.Email))
            {
                //Il existe un adhérent avec cet @ mail
                if (_context.AdherentStolons.Include(x => x.Adherent).Any(x => x.Adherent.Email == vmAdherent.Adherent.Email && vmAdherent.Stolon.Id == x.StolonId)) // et il est déjà de ce stolon
                {
                    //Il est déjà dans ce stolon, on renvoid
                    return RedirectToAction("Index");
                }
                else
                {
                    //Il est dans un autre stolon, alors on l'ajoute à ce stolon
                    var selectAdherentVm = new SelectAdherentViewModel(GetActiveAdherentStolon(), vmAdherent.Stolon, null, vmAdherent.Edition == AdherentEdition.Producer);
                    selectAdherentVm.SelectedEmail = vmAdherent.Adherent.Email;
                    return AddAdherent(selectAdherentVm);
                }
            }

            if (ModelState.IsValid)
            {
                Stolon stolon = _context.Stolons.FirstOrDefault(x => x.Id == vmAdherent.Stolon.Id);
                #region Creating Consumer
                //Setting value for creation
                UploadAndSetAvatar(vmAdherent.Adherent, uploadFile);

                vmAdherent.Adherent.Name = vmAdherent.Adherent.Name.ToUpper();
                AdherentStolon adherentStolon = new AdherentStolon(vmAdherent.Adherent, stolon, true);
                adherentStolon.RegistrationDate = DateTime.Now;
                adherentStolon.LocalId = _context.AdherentStolons.Where(x => x.StolonId == stolon.Id).Max(x => x.LocalId) + 1;
                adherentStolon.IsProducer = edition == AdherentEdition.Producer;
                _context.Adherents.Add(vmAdherent.Adherent);
                _context.AdherentStolons.Add(adherentStolon);
                #endregion Creating Consumer

                #region Creating linked application data
                var appUser = new ApplicationUser { UserName = vmAdherent.Adherent.Email, Email = vmAdherent.Adherent.Email };
                appUser.User = vmAdherent.Adherent;

                var result = await _userManager.CreateAsync(appUser, vmAdherent.Adherent.Email);

                #endregion Creating linked application data
                //
                //
                _context.SaveChanges();
                //Send confirmation mail
                string confirmationViewName = edition == AdherentEdition.Consumer ? "AdherentConfirmationMail" : "ProducerConfirmationMail";
                Services.AuthMessageSender.SendEmail(stolon.Label, vmAdherent.Adherent.Email, vmAdherent.Adherent.Name, "Creation de votre compte", base.RenderPartialViewToString(confirmationViewName, adherentStolon));

                return RedirectToAction("Index");
            }
            return View(vmAdherent);
        }

        public PartialViewResult _PartialEditAdherent(Guid id)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).FirstOrDefault(x => x.Id == id);

            return PartialView(new AdherentViewModel(GetActiveAdherentStolon(), adherentStolon.Adherent, adherentStolon.Stolon, adherentStolon.IsProducer ? AdherentEdition.Producer : AdherentEdition.Consumer));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAdherent(AdherentViewModel vmAdherent, string uploadAvatar)
        {
            if (Authorized(Role.Volunteer) || vmAdherent.Adherent.Id == GetActiveAdherentStolon().AdherentId)
            {
                if (ModelState.IsValid)
                {
                    UpdateAdherent(vmAdherent, uploadAvatar);
                    return RedirectToAction("Index");
                }
                return View(vmAdherent);

            }
            return Unauthorized();

        }

        public void UpdateAdherent(AdherentViewModel vmAdherent, string uploadAvatar)
        {
            //Change mail if different
            ApplicationUser appUser = _context.Users.First(x => x.Email == vmAdherent.OriginalEmail);
            if(appUser.Email != vmAdherent.Adherent.Email)
            {
                appUser.Email = vmAdherent.Adherent.Email;
                _context.Update(appUser);
                _context.SaveChanges();
            }

            _environment.DeleteFile(vmAdherent.Adherent.AvatarFilePath);
            vmAdherent.Adherent.AvatarFileName = _environment.UploadBase64Image(uploadAvatar, Configurations.AvatarStockagePath);

            Adherent adherent = _context.Adherents.FirstOrDefault(x => x.Id == vmAdherent.Adherent.Id);
            adherent.CloneAllPropertiesFrom(vmAdherent.Adherent);
            _context.Update(adherent);
            _context.SaveChanges();
        }


        public virtual IActionResult DeleteAdherent(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x=>x.Adherent).First(x => x.Id == id);
            adherentStolon.Deleted = true;
            if (!_context.AdherentStolons.Any(x => x.AdherentId == adherentStolon.AdherentId && x.Deleted == false))
            {
                //On supprime son mail de connexion
                _userManager.DeleteAsync(_userManager.Users.First(x => x.Email == adherentStolon.Adherent.Email));
                //L'adhérent n'est dans aucun stolon, on le "supprime définitivement" (on l'anonymise)
                adherentStolon.Adherent.Name = "(Supprimer)";
                adherentStolon.Adherent.Surname = "(Supprimer)";
                adherentStolon.Adherent.PostCode = "";
                adherentStolon.Adherent.Address = "";
                adherentStolon.Adherent.City = "";
                adherentStolon.Adherent.Email = "";
                adherentStolon.Adherent.AvatarFileName = "";
                adherentStolon.Adherent.PhoneNumber = "";
            }
            else
            {
                var adherentStolons = _context.AdherentStolons.Include(x => x.Adherent).Where(x => x.AdherentId == adherentStolon.AdherentId && !x.Deleted && x.Id != adherentStolon.Id);
                if (adherentStolons.Any())
                    adherentStolons.First().SetHasActiveStolon(_context);
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Pay subscription
        /// </summary>
        /// <param name="id">AdherentStolon id</param>
        /// <returns></returns>
        public IActionResult PaySubscription(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();
            //Adherent stolon
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Stolon).Include(x => x.Adherent).First(x => x.Id == id);

            adherentStolon.SubscriptionPaid = true;
            _context.AdherentStolons.Update(adherentStolon);
            //Transaction
            Transaction transaction = new Transaction();
            transaction = new AdherentTransaction() { Adherent = adherentStolon.Adherent };
            //Update
            transaction.Amount = Configurations.GetSubscriptionAmount(adherentStolon);
            //Add a transaction
            transaction.Stolon = adherentStolon.Stolon;
            transaction.AddedAutomaticly = true;
            transaction.Date = DateTime.Now;
            transaction.Type = Transaction.TransactionType.Inbound;
            transaction.Category = Transaction.TransactionCategory.Subscription;
            transaction.Description = "Paiement de la cotisation de l'adhérant " + (adherentStolon.IsProducer ? "producteur" : "") + " : " + adherentStolon.Adherent.Name + " " + adherentStolon.Adherent.Surname;
            _context.Transactions.Add(transaction);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult EnableAdherent(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);

            adherentStolon.DisableReason = "";
            adherentStolon.Enable = true;
            //Update
            _context.AdherentStolons.Update(adherentStolon);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        public virtual PartialViewResult _PartialDisableAdherent(Guid id, Guid? stolonId = null)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).FirstOrDefault(x => x.Id == id);


            return PartialView(new DisableAccountViewModel(GetActiveAdherentStolon(), adherentStolon));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual IActionResult DisableAdherent(AdherentStolonViewModel vmAdherentStolon, string comment)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();
            //
            var adherentStolon = _context.AdherentStolons.FirstOrDefault(x => x.Id == vmAdherentStolon.AdherentStolon.Id);
            adherentStolon.DisableReason = comment;
            adherentStolon.Enable = false;
            if(adherentStolon.IsProducer)
            {
                _context.ProductsStocks.Where(x => x.AdherentStolonId == adherentStolon.Id).ToList().ForEach(x => x.State = Product.ProductState.Disabled);
            }
            //Update
            _context.AdherentStolons.Update(adherentStolon);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public PartialViewResult _PartialCreditToken(Guid id)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).Include(x => x.Stolon).FirstOrDefault(x => x.Id == id);
            return PartialView(new CreditTokenViewModel(GetActiveAdherentStolon(), adherentStolon));
        }

        // POST: Consumers/CreditToken
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreditToken(CreditTokenViewModel vmCreditToken)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).First(x => x.Id == vmCreditToken.AdherentStolon.Id);
            adherentStolon.Token += vmCreditToken.CreditedToken;
            _context.Add(new AdherentTransaction(
                adherentStolon.Adherent,
                adherentStolon.Stolon,
                Transaction.TransactionType.Inbound,
                Transaction.TransactionCategory.TokenCredit,
                vmCreditToken.CreditedToken,
                "Encaissement de " + vmCreditToken.CreditedToken + "€, pour créditage du compte de " + adherentStolon.Adherent.Name + "( " + adherentStolon.LocalId + " ) de " + vmCreditToken.CreditedToken + "Ṩ"));
            _context.Update(adherentStolon);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }





        #region Sympathizers


        public PartialViewResult _PartialDetailsSympathizer(Guid id)
        {
            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            return PartialView(new SympathizerViewModel(GetActiveAdherentStolon(), sympathizer));
        }


        public PartialViewResult _PartialCreateSympathizer(Guid stolonId)
        {
            return PartialView(new SympathizerViewModel(GetActiveAdherentStolon(), new Sympathizer() { StolonId = stolonId }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSympathizer(SympathizerViewModel vmSympathizer)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                #region Creating Sympathizer
                string fileName = Configurations.DefaultImageFileName;

                //Setting value for creation
                vmSympathizer.Sympathizer.RegistrationDate = DateTime.Now;
                _context.Sympathizers.Add(vmSympathizer.Sympathizer);
                #endregion Creating Sympathizer

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vmSympathizer);
        }


        public PartialViewResult _PartialEditSympathizer(Guid id)
        {
            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);

            return PartialView(new SympathizerViewModel(GetActiveAdherentStolon(), sympathizer));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSympathizer(SympathizerViewModel vmSympathizer)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            if (ModelState.IsValid)
            {
                _context.Update(vmSympathizer.Sympathizer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vmSympathizer);
        }

        public IActionResult DeleteSympathizer(Guid id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();

            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            _context.Sympathizers.Remove(sympathizer);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// PaySympathiserSubscription
        /// </summary>
        /// <param name="id">Sympathizer ID</param>
        /// <returns></returns>
        public IActionResult PaySympathiserSubscription(Guid id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();

            Transaction transaction = new Transaction();
            Sympathizer sympathizer = _context.Sympathizers.FirstOrDefault(x => x.Id == id);
            Stolon currentStolon = GetCurrentStolon();
            sympathizer.SubscriptionPaid = true;
            transaction.Amount = currentStolon.SympathizerSubscription;
            //Add a transaction
            transaction.Stolon = currentStolon;
            transaction.AddedAutomaticly = true;
            transaction.Date = DateTime.Now;
            transaction.Type = Transaction.TransactionType.Inbound;
            transaction.Category = Transaction.TransactionCategory.Subscription;
            transaction.Description = "Paiement de la cotisation du sympathisant : " + sympathizer.Name + " " + sympathizer.Surname;
            _context.Transactions.Add(transaction);
            //Save
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        #endregion Sympathizers

        #region Roles
        public IActionResult SetAsAdherent(Guid? id)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);
            adherentStolon.Role = Role.Adherent;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult SetAsVolunteer(Guid? id)
        {
            if (!Authorized(Role.Volunteer))
                return Unauthorized();
            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);
            adherentStolon.Role = Role.Volunteer;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult SetAsAdmin(Guid? id)
        {
            if (!Authorized(Role.Admin))
                return Unauthorized();
            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);
            adherentStolon.Role = Role.Admin;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult SetAsWebAdmin(Guid? id)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).First(x => x.Id == id);
            adherentStolon.Adherent.IsWebAdmin = true;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UnSetAsWebAdmin(Guid? id)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x => x.Adherent).First(x => x.Id == id);
            adherentStolon.Adherent.IsWebAdmin = false;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Set as producer the specified adherent stolon
        /// </summary>
        /// <param name="id">Adherent Stolon Id</param>
        public IActionResult SetAsProducer(Guid? id)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.Include(x=>x.Stolon).Include(x => x.Adherent).ThenInclude(x => x.Products).First(x => x.Id == id);
            adherentStolon.IsProducer = true;
            adherentStolon.ProducerFee = adherentStolon.Stolon.DefaultProducersFee;
            foreach (var product in adherentStolon.Adherent.Products)
            {
                _context.ProductsStocks.Add(new ProductStockStolon(product.Id, adherentStolon.Id));
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult SetAsConsumer(Guid? id)
        {
            AdherentStolon adherentStolon = _context.AdherentStolons.First(x => x.Id == id);
            adherentStolon.IsProducer = false;
            _context.ProductsStocks.RemoveRange(_context.ProductsStocks.Where(x => x.AdherentStolonId == adherentStolon.Id));
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion Roles

    }
}
