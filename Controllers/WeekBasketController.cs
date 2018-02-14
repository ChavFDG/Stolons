using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Text;
using MimeKit;
using System.IO;
using Newtonsoft.Json;
using Stolons.Models;
using Stolons.ViewModels.WeekBasket;
using System;
using Stolons.Models.Users;

namespace Stolons.Controllers
{
    [Authorize]
    public class WeekBasketController : BaseController
    {

        public WeekBasketController(ApplicationDbContext context, IHostingEnvironment environment,
				    UserManager<ApplicationUser> userManager,
				    SignInManager<ApplicationUser> signInManager,
				    IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {
        }

        // GET: WeekBasket/Index/id
        public IActionResult Index()
        {
            AdherentStolon adherentStolon = GetActiveAdherentStolon();
            if (adherentStolon == null)
            {
                return NotFound();
            }

            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).FirstOrDefault(x => x.AdherentStolon.Id == adherentStolon.Id);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).FirstOrDefault(x => x.AdherentStolon.Id == adherentStolon.Id);
            if (tempWeekBasket == null)
            {
                //Il n'a pas encore de panier de la semaine, on lui en crÃ©e un
                tempWeekBasket = new TempWeekBasket
                {
                    AdherentStolon = adherentStolon,
                    BillEntries = new List<BillEntry>()
                };
                _context.Add(tempWeekBasket);
                _context.SaveChanges();
            }
            return View(new WeekBasketViewModel(adherentStolon, tempWeekBasket, validatedWeekBasket, _context));
        }

        [AllowAnonymous]
        [HttpGet, ActionName("Products"), Route("api/products")]
        public IActionResult JsonProductsStocks()
        {
            var productsStocks = _context.ProductsStocks
		.Include(x => x.Product)
		.ThenInclude(x => x.Familly)
		.ThenInclude(x => x.Type)
		.Where(x => x.AdherentStolon.StolonId == GetCurrentStolon().Id && x.State == Product.ProductState.Enabled)
		.AsNoTracking()
		.ToList();
            foreach (ProductStockStolon p in productsStocks)
            {
                p.Product.Producer = _context.Adherents.AsNoTracking().FirstOrDefault(x => x.Id == p.Product.ProducerId);
            }
            return Json(productsStocks);
        }

        [AllowAnonymous]
        [HttpGet, ActionName("PublicProducts"), Route("api/publicProducts")]
        public IActionResult JsonPublicProductsStocks()
        {
            var productsStocks = _context.ProductsStocks
		.Include(x => x.AdherentStolon)
		.ThenInclude(x => x.Adherent)
		.Include(x => x.AdherentStolon)
		.Include(x => x.Product)
		.ThenInclude(x => x.Familly)
		.ThenInclude(x => x.Type)
		.Where(x => x.AdherentStolon.StolonId == GetCurrentStolon().Id)
		.AsNoTracking()
		.ToList();
            return Json(productsStocks);
        }

        [AllowAnonymous]
        [HttpGet, ActionName("ProductTypes"), Route("api/productTypes")]
        public IActionResult JsonProductTypes()
        {
            var ProductTypes = _context.ProductTypes
		.Include(x => x.ProductFamilly)
		.AsNoTracking()
		.ToList();
            return Json(ProductTypes);
        }

        [HttpGet, ActionName("TmpWeekBasket"), Route("api/tmpWeekBasket")]
        public IActionResult JsonTmpWeekBasket()
        {
            var adherentStolon = GetActiveAdherentStolon();
            if (adherentStolon == null)
            {
                return null;
            }
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.BillEntries).FirstOrDefault(x => x.AdherentStolon.Id == adherentStolon.Id);
            if (tempWeekBasket == null)
            {
                //Il n'a pas encore de panier de la semaine, on lui en creer un
                tempWeekBasket = new TempWeekBasket
                {
                    AdherentStolon = adherentStolon,
                    BillEntries = new List<BillEntry>()
                };
                _context.Add(tempWeekBasket);
                _context.SaveChanges();
            }
            else
            {
                tempWeekBasket.RetrieveProducts(_context);
            }
	    tempWeekBasket.Validated = IsBasketValidated(tempWeekBasket);
            return Json(tempWeekBasket);
        }

        [HttpGet, ActionName("ValidatedWeekBasket"), Route("api/validatedWeekBasket")]
        public IActionResult JsonValidatedWeekBasket()
        {
            var adherentStolon = GetActiveAdherentStolon();

            if (adherentStolon == null)
            {
                return null;
            }
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets
		.Include(x => x.AdherentStolon)
		.Include(x => x.AdherentStolon.Adherent)
		.Include(x => x.BillEntries)
		.AsNoTracking()
		.FirstOrDefault(x => x.AdherentStolon.Id == adherentStolon.Id);

            if (validatedWeekBasket != null)
            {
                validatedWeekBasket.RetrieveProducts(_context);
            }
            return Json(validatedWeekBasket);
        }

        [HttpPost, ActionName("AddToBasket"), Route("api/addToBasket")]
        public IActionResult AddToBasket(string weekBasketId, string productStockId)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).AsNoTracking().First(x => x.Id.ToString() == weekBasketId);
            if (tempWeekBasket.BillEntries.Any(x => x.ProductStockId.ToString() == productStockId))
                return JsonTmpWeekBasket();//On a déjà le produit
            ProductStockStolon ProductStock = _context.ProductsStocks.Include(x => x.Product).ThenInclude(x => x.Producer).Single(x => x.Id.ToString() == productStockId);
            BillEntry billEntry = BillEntry.CloneFromProduct(ProductStock);
            billEntry.ProductStockId = ProductStock.Id;
            billEntry.Quantity = 1;
            billEntry.TempWeekBasketId = tempWeekBasket.Id;
            _context.Add(billEntry);
            _context.SaveChanges();
            return JsonTmpWeekBasket();
        }

        [HttpPost, ActionName("PlusProduct"), Route("api/incrementProduct")]
        public IActionResult PlusProduct(string weekBasketId, string productStockId)
        {
            AddProductQuantity(weekBasketId, productStockId, +1);
            return JsonTmpWeekBasket();
        }

        [HttpPost, ActionName("MinusProduct"), Route("api/decrementProduct")]
        public IActionResult MinusProduct(string weekBasketId, string productStockId)
        {
            AddProductQuantity(weekBasketId, productStockId, -1);
            return JsonTmpWeekBasket();
        }

        private TempWeekBasket AddProductQuantity(string weekBasketId, string productStockId, int quantity)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).First(x => x.Id.ToString() == weekBasketId);
            //tempWeekBasket.RetrieveProducts(_context);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).AsNoTracking().FirstOrDefault(x => x.Adherent.Id == tempWeekBasket.Adherent.Id);

            int validatedQuantity = 0;
            if (validatedWeekBasket != null)
            {
                BillEntry validatedEntry = validatedWeekBasket.BillEntries.FirstOrDefault(x => x.ProductStockId.ToString() == productStockId);

                if (validatedEntry != null)
                {
                    validatedQuantity = validatedEntry.Quantity;
                }
            }
            BillEntry billEntry = tempWeekBasket.BillEntries.FirstOrDefault(x => x.ProductStockId.ToString() == productStockId);
            ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).FirstOrDefault(x => x.Id.ToString() == productStockId);

            decimal stepStock = productStock.RemainingStock;
            if (productStock.Product.Type != Product.SellType.Piece)
            {
                stepStock = (productStock.RemainingStock * 1000.0M) / productStock.Product.QuantityStep;
            }
            bool proceed = false;
            if (productStock.Product.StockManagement == Product.StockType.Unlimited)
            {
                proceed = true;
            }
            if (!(quantity > 0 && stepStock < (billEntry.Quantity - validatedQuantity) + quantity))
            {
                proceed = true;
            }
            if (proceed == true)
            {
                billEntry.Quantity = billEntry.Quantity + quantity;

                if (billEntry.Quantity <= 0)
                {
                    //La quantite est 0 on supprime le produit
                    tempWeekBasket.BillEntries.Remove(billEntry);
                    _context.Remove(billEntry);
                }
                _context.SaveChanges();
            }
            return tempWeekBasket;
        }

        [HttpPost, ActionName("RemoveBillEntry"), Route("api/removeBillEntry")]
        public IActionResult RemoveBillEntry(string weekBasketId, string productStockId)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.BillEntries).ThenInclude(x => x.ProductStock).ThenInclude(x => x.Product).First(x => x.Id.ToString() == weekBasketId);
            tempWeekBasket.RetrieveProducts(_context);
            BillEntry billEntry = tempWeekBasket.BillEntries.First(x => x.ProductId.ToString() == productStockId);
            tempWeekBasket.BillEntries.Remove(billEntry);
            _context.Remove(billEntry);
            _context.SaveChanges();
            return JsonTmpWeekBasket();
        }

        //Unused for now
        [HttpPost, ActionName("ResetBasket"), Route("api/resetBasket")]
        public IActionResult ResetBasket(string basketId)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).First(x => x.Id.ToString() == basketId);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).FirstOrDefault(x => x.Adherent.Id == tempWeekBasket.Adherent.Id);

            if (validatedWeekBasket == null)
            {
                tempWeekBasket.BillEntries = new List<BillEntry>();
            }
            else
            {
                tempWeekBasket.BillEntries = new List<BillEntry>();
                foreach (BillEntry billEntry in validatedWeekBasket.BillEntries.ToList())
                {
                    tempWeekBasket.BillEntries.Add(billEntry.Clone());
                }
            }
            tempWeekBasket.Validated = true;
            _context.SaveChanges();
            tempWeekBasket.RetrieveProducts(_context);
            return Json(tempWeekBasket);
        }

        /**
         * Updates product remaining stock with the given quantity (< 0 || > 0)
         * Manages the stock according to the sell type of the product.
         */
        private void UpdateProductStock(ProductStockStolon productStock, int qty)
        {
            productStock.RemainingStock += qty;
        }

        public IActionResult Basket()
        {
            Stolon stolon = GetCurrentStolon();
            // if (stolon.GetMode() == Stolon.Modes.DeliveryAndStockUpdate)
            //     return Redirect("Index");
            var adherentStolon = GetActiveAdherentStolon();
            if (adherentStolon == null)
            {
                return null;
            }
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).FirstOrDefault(x => x.AdherentStolon.Id == adherentStolon.Id);
            ValidationSummaryViewModel validationSummaryViewModel = new ValidationSummaryViewModel(GetActiveAdherentStolon(), validatedWeekBasket, new List<BillEntry>()) { Total = GetBasketPrice(validatedWeekBasket) };
            return View("Basket", validationSummaryViewModel);
        }

        [HttpGet, ActionName("ValidateBasket")]
        public IActionResult ValidateBasket()
        {
            return View("Basket");
        }

        [HttpPost, ActionName("ValidateBasket")]
        public IActionResult ValidateBasket(string basketId)
        {
            var adherentStolon = GetActiveAdherentStolon();
            Stolon stolon = GetCurrentStolon();
            if (stolon.GetMode() == Stolon.Modes.DeliveryAndStockUpdate)
                return Redirect("Index");
            //TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.BillEntries).Include(x => x.AdherentStolon).AsNoTracking().FirstOrDefault(x => x.Id.ToString() == basketId);
            //tempWeekBasket.RetrieveProducts(_context);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.AdherentStolon).ThenInclude(x => x.Adherent).Include(x => x.BillEntries).FirstOrDefault(x => x.AdherentStolonId == adherentStolon.Id);

            if (validatedWeekBasket == null)
            {
                //First validation of the week
                validatedWeekBasket = new ValidatedWeekBasket
                {
                    BillEntries = new List<BillEntry>(),
                    AdherentStolon = adherentStolon
                };
                _context.Add(validatedWeekBasket);
                _context.SaveChanges();
            }
            else
            {
                validatedWeekBasket.RetrieveProducts(_context);
            }
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.BillEntries).Include(x => x.AdherentStolon).FirstOrDefault(x => x.Id.ToString() == basketId);
            tempWeekBasket.RetrieveProducts(_context);
            //TODO LOCK to prevent multi insert at this moment ?
            if (tempWeekBasket.BillEntries.Any())
            {
                List<BillEntry> rejectedEntries = new List<BillEntry>();
                //Sauvegarde des produits déja validés
                List<BillEntry> previousBillEntries = validatedWeekBasket.BillEntries;
                //On met le panier validé dans le même état que le temporaire
                validatedWeekBasket.BillEntries = new List<BillEntry>();
                foreach (BillEntry billEntry in tempWeekBasket.BillEntries.ToList())
                {
                    validatedWeekBasket.BillEntries.Add(billEntry.Clone());
                }

                //Gestion de la suppression et du changement de quantité sur des billEntry existantes
                foreach (BillEntry prevEntry in previousBillEntries)
                {
                    BillEntry newEntry = validatedWeekBasket.BillEntries.FirstOrDefault(x => x.ProductStockId == prevEntry.ProductStockId);
                    ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).Single(x => x.Id == prevEntry.ProductStockId);

                    if (newEntry == null)
                    {
                        //produit supprimé du panier
                        UpdateProductStock(productStock, prevEntry.Quantity);
                    }
                    else
                    {
                        int qtyDiff = newEntry.Quantity - prevEntry.Quantity;
                        decimal stepStock = productStock.RemainingStock;
                        if (productStock.Product.Type != Product.SellType.Piece)
                        {
                            //Actual remaining stock in terms of quantity step Kg/L for weight type products
                            stepStock = (productStock.RemainingStock / productStock.Product.QuantityStep) * 1000.0M;
                        }
                        if (stepStock < qtyDiff && productStock.Product.StockManagement != Product.StockType.Unlimited)
                        {
                            //Stock insuffisant, on supprime la nouvelle ligne et on garde l'ancienne
                            validatedWeekBasket.BillEntries.Remove(newEntry);
                            validatedWeekBasket.BillEntries.Add(prevEntry);
                            rejectedEntries.Add(newEntry);
                        }
                        else
                        {
                            UpdateProductStock(productStock, -qtyDiff);
                            //On supprime la bill entry précédente ( ancienne bill entry)
                            _context.BillEntrys.Remove(prevEntry);
                        }
                    }
                }

                //Gestion de l'ajout de produits
                foreach (BillEntry newEntry in validatedWeekBasket.BillEntries.ToList())
                {
                    BillEntry prevEntry = previousBillEntries.FirstOrDefault(x => x.ProductStockId == newEntry.ProductStockId);

                    if (prevEntry == null)
                    {
                        //Nouveau produit
                        ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).Single(x => x.Id == newEntry.ProductStockId);

                        decimal stepStock = productStock.RemainingStock;
                        if (productStock.Product.Type != Product.SellType.Piece)
                        {
                            stepStock = (productStock.RemainingStock / productStock.Product.QuantityStep) * 1000.0M;
                        }
                        if (newEntry.Quantity <= stepStock || productStock.Product.StockManagement == Product.StockType.Unlimited)
                        {
                            //product.RemainingStock -= newEntry.Quantity;
                            UpdateProductStock(productStock, -newEntry.Quantity);
                        }
                        else
                        {
                            validatedWeekBasket.BillEntries.Remove(newEntry);
                            rejectedEntries.Add(newEntry);
                        }
                    }
                }

                _context.SaveChanges();
                //On supprime toute les BillEntry du tempWeekBasket
                _context.BillEntrys.RemoveRange(_context.BillEntrys.Where(x => x.TempWeekBasketId == tempWeekBasket.Id).ToList());
                _context.SaveChanges();
                //On met le panier temporaire dans le même état que le validé
                foreach (BillEntry entry in validatedWeekBasket.BillEntries)
                {
                    tempWeekBasket.BillEntries.Add(entry.Clone());
                }
                tempWeekBasket.Validated = true;

                _context.SaveChanges();
                //END LOCK TODO

                //Recuperation du detail produit pour utilisation dans la Vue
                validatedWeekBasket.RetrieveProducts(_context);

                //Send email to user
                string subject;
                if (rejectedEntries.Count == 0)
                {
                    subject = "Validation de votre panier de la semaine";
                }
                else
                {
                    subject = "Validation partielle de votre panier de la semaine";
                }
                ValidationSummaryViewModel validationSummaryViewModel = new ValidationSummaryViewModel(adherentStolon, validatedWeekBasket, rejectedEntries) { Total = GetBasketPrice(validatedWeekBasket) };
                Services.AuthMessageSender.SendEmail(adherentStolon.Stolon.Label, validatedWeekBasket.Adherent.Email, validatedWeekBasket.Adherent.Name, subject, base.RenderPartialViewToString("Templates/ValidatedBasketTemplate", validationSummaryViewModel));
                //Return view
                return View("Basket", validationSummaryViewModel);
            }
            else
            {
                //On annule tout le contenu du panier
                foreach (BillEntry entry in validatedWeekBasket.BillEntries)
                {
                    ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).Single(x => x.Id == entry.ProductStockId);
                    UpdateProductStock(productStock, entry.Quantity);
                    //entry.Product.RemainingStock += entry.Quantity;
                }
                _context.Remove(tempWeekBasket);
                _context.Remove(validatedWeekBasket);
                _context.SaveChanges();

                //Il ne commande rien du tout
                //On lui signale
                Services.AuthMessageSender.SendEmail(stolon.Label, validatedWeekBasket.Adherent.Email, validatedWeekBasket.Adherent.Name, "Panier de la semaine annulé", base.RenderPartialViewToString("Templates/ValidatedBasketTemplate", null));
            }
            return View("Basket");
        }

        //Calcul du prix total d'un panier
        private Decimal GetBasketPrice(IWeekBasket basket)
        {
            if (basket == null)
            {
                return 0;
            }
            Decimal price = 0;
            foreach (BillEntry entry in basket.BillEntries)
            {
                price += entry.Price;
            }
            return price;
        }

        private bool IsBasketValidated(TempWeekBasket tmpBasket)
        {
            ValidatedWeekBasket validatedBasket = _context.ValidatedWeekBaskets.Include(x => x.AdherentStolon).Include(x => x.AdherentStolon.Adherent).Include(x => x.BillEntries).AsNoTracking().FirstOrDefault(x => x.Adherent.Id == tmpBasket.Adherent.Id);

            if (validatedBasket == null)
            {
                return false;
            }
            if (validatedBasket.BillEntries.Count != tmpBasket.BillEntries.Count)
            {
                return false;
            }
            foreach (BillEntry billEntry in tmpBasket.BillEntries.ToList())
            {
                BillEntry validatedEntry = validatedBasket.BillEntries.FirstOrDefault(x => x.ProductStockId == billEntry.ProductStockId);

                if (validatedEntry == null)
                {
                    return false;
                }
                if (billEntry.Quantity != validatedEntry.Quantity)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
