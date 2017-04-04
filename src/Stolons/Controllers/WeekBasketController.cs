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
        public async Task<IActionResult> Index()
        {
            Adherent consumer = await GetCurrentAdherentAsync() as Adherent;
            if (consumer == null)
            {
                return NotFound();
            }
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == consumer.Id);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == consumer.Id);
            if (tempWeekBasket == null)
            {
                //Il n'a pas encore de panier de la semaine, on lui en crÃ©e un
                tempWeekBasket = new TempWeekBasket();
                tempWeekBasket.Consumer = consumer;
                tempWeekBasket.Products = new List<BillEntry>();
                _context.Add(tempWeekBasket);
                _context.SaveChanges();
            }
            return View(new WeekBasketViewModel(GetActiveAdherentStolon(), tempWeekBasket, validatedWeekBasket, _context));
        }

        [AllowAnonymous]
        [HttpGet, ActionName("Products"), Route("api/products")]
        public string JsonProductsStocks()
        {
            var productsStocks = _context.ProductsStocks
                                            .Include(x => x.AdherentStolon)
                                                .ThenInclude(x => x.Adherent)
                                            .Include(x => x.AdherentStolon)
                                            .Include(x => x.Product)
                                                .ThenInclude(x => x.Familly)
                                                    .ThenInclude(x => x.Type)
                                            .Where(x => x.AdherentStolon.StolonId == GetCurrentStolon().Id && x.State == Product.ProductState.Enabled).ToList();
            return JsonConvert.SerializeObject(productsStocks, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [AllowAnonymous]
        [HttpGet, ActionName("PublicProducts"), Route("api/publicProducts")]
        public string JsonPublicProductsStocks()
        {
            var productsStocks = _context.ProductsStocks
                                             .Include(x => x.AdherentStolon)
                                                 .ThenInclude(x => x.Adherent)
                                             .Include(x => x.AdherentStolon)
                                             .Include(x => x.Product)
                                                 .ThenInclude(x => x.Familly)
                                                     .ThenInclude(x => x.Type)
                                             .Where(x => x.AdherentStolon.StolonId == GetCurrentStolon().Id).ToList();
            return JsonConvert.SerializeObject(productsStocks, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [AllowAnonymous]
        [HttpGet, ActionName("ProductTypes"), Route("api/productTypes")]
        public string JsonProductTypes()
        {
            var ProductTypes = _context.ProductTypes.Include(x => x.ProductFamilly).ToList();
            return JsonConvert.SerializeObject(ProductTypes, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet, ActionName("TmpWeekBasket"), Route("api/tmpWeekBasket")]
        public async Task<string> JsonTmpWeekBasket()
        {
            Adherent consumer = await GetCurrentAdherentAsync() as Adherent;
            if (consumer == null)
            {
                return null;
            }
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == consumer.Id);
            if (tempWeekBasket == null)
            {
                //Il n'a pas encore de panier de la semaine, on lui en creer un
                tempWeekBasket = new TempWeekBasket();
                tempWeekBasket.Consumer = consumer;
                tempWeekBasket.Products = new List<BillEntry>();
                _context.Add(tempWeekBasket);
                _context.SaveChanges();
            }
            else
            {
                tempWeekBasket.RetrieveProducts(_context);
            }
            return JsonConvert.SerializeObject(tempWeekBasket, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet, ActionName("ValidatedWeekBasket"), Route("api/validatedWeekBasket")]
        public async Task<string> JsonValidatedWeekBasket()
        {
            Adherent consumer = await GetCurrentAdherentAsync() as Adherent;
            if (consumer == null)
            {
                return null;
            }
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == consumer.Id);
            if (validatedWeekBasket != null)
            {
                validatedWeekBasket.RetrieveProducts(_context);
            }
            return JsonConvert.SerializeObject(validatedWeekBasket, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost, ActionName("AddToBasket"), Route("api/addToBasket")]
        public string AddToBasket(string weekBasketId, string productId)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).First(x => x.Id.ToString() == weekBasketId);
            tempWeekBasket.RetrieveProducts(_context);
            BillEntry billEntry = new BillEntry();
            billEntry.Product = _context.Products.Include(x => x.Producer).ThenInclude(x => x.AdherentStolons).First(x => x.Id.ToString() == productId);
            billEntry.ProductId = billEntry.Product.Id;
            billEntry.Quantity = 1;
            tempWeekBasket.Products.Add(billEntry);
            tempWeekBasket.Validated = IsBasketValidated(tempWeekBasket);
            _context.SaveChanges();
            return JsonConvert.SerializeObject(tempWeekBasket, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost, ActionName("PlusProduct"), Route("api/incrementProduct")]
        public string PlusProduct(string weekBasketId, string productId)
        {
            return JsonConvert.SerializeObject(AddProductQuantity(weekBasketId, productId, +1), Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [HttpPost, ActionName("MinusProduct"), Route("api/decrementProduct")]
        public string MinusProduct(string weekBasketId, string productId)
        {
            return JsonConvert.SerializeObject(AddProductQuantity(weekBasketId, productId, -1), Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        private TempWeekBasket AddProductQuantity(string weekBasketId, string productStockId, int quantity)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).First(x => x.Id.ToString() == weekBasketId);
            tempWeekBasket.RetrieveProducts(_context);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == tempWeekBasket.Consumer.Id);

            int validatedQuantity = 0;
            if (validatedWeekBasket != null)
            {
                BillEntry validatedEntry = validatedWeekBasket.Products.FirstOrDefault(x => x.ProductId.ToString() == productStockId);

                if (validatedEntry != null)
                {
                    validatedQuantity = validatedEntry.Quantity;
                }
            }
            BillEntry billEntry = tempWeekBasket.Products.FirstOrDefault(x => x.ProductId.ToString() == productStockId);
            ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).FirstOrDefault(x => x.Id.ToString() == productStockId);

            decimal stepStock = productStock.RemainingStock;
            if (productStock.Product.Type != Product.SellType.Piece)
            {
                stepStock = (productStock.RemainingStock * 1000.0M) / productStock.Product.QuantityStep;
            }
            if (!(quantity > 0 && stepStock < (billEntry.Quantity - validatedQuantity) + quantity))
            {
                billEntry.Quantity = billEntry.Quantity + quantity;

                if (billEntry.Quantity <= 0)
                {
                    //La quantite est 0 on supprime le produit
                    tempWeekBasket.Products.Remove(billEntry);
                    _context.Remove(billEntry);
                }
                tempWeekBasket.Validated = IsBasketValidated(tempWeekBasket);
                _context.SaveChanges();
            }
            return tempWeekBasket;
        }

        [HttpPost, ActionName("RemoveBillEntry"), Route("api/removeBillEntry")]
        public string RemoveBillEntry(string weekBasketId, string productId)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).First(x => x.Id.ToString() == weekBasketId);
            tempWeekBasket.RetrieveProducts(_context);
            BillEntry billEntry = tempWeekBasket.Products.First(x => x.ProductId.ToString() == productId);
            tempWeekBasket.Products.Remove(billEntry);
            _context.Remove(billEntry);
            tempWeekBasket.Validated = IsBasketValidated(tempWeekBasket);
            _context.SaveChanges();
            return JsonConvert.SerializeObject(tempWeekBasket, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        //Unused for now
        [HttpPost, ActionName("ResetBasket"), Route("api/resetBasket")]
        public string ResetBasket(string basketId)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Products).Include(x => x.Consumer).First(x => x.Id.ToString() == basketId);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == tempWeekBasket.Consumer.Id);

            if (validatedWeekBasket == null)
            {
                tempWeekBasket.Products = new List<BillEntry>();
            }
            else
            {
                tempWeekBasket.Products = new List<BillEntry>();
                foreach (BillEntry billEntry in validatedWeekBasket.Products.ToList())
                {
                    tempWeekBasket.Products.Add(billEntry.Clone());
                }
            }
            tempWeekBasket.Validated = true;
            _context.SaveChanges();
            tempWeekBasket.RetrieveProducts(_context);
            return JsonConvert.SerializeObject(tempWeekBasket, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        /**
         * Updates product remaining stock with the given quantity (< 0 || > 0)
         * Manages the stock according to the sell type of the product.
         */
        private void UpdateProductStock(ProductStockStolon productStock, int qty)
        {
            if (productStock.Product.Type == Product.SellType.Piece)
            {
                productStock.RemainingStock += qty;
            }
            else
            {
                productStock.RemainingStock += ((decimal)((decimal)qty * (decimal)productStock.Product.QuantityStep)) / 1000.0M;
            }
        }

        [HttpPost, ActionName("ValidateBasket")]
        public IActionResult ValidateBasket(string basketId)
        {
            Stolon stolon = GetCurrentStolon();
            if (stolon.GetMode() == Stolon.Modes.DeliveryAndStockUpdate)
                return Redirect("Index");

            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Products).Include(x => x.Consumer).First(x => x.Id.ToString() == basketId);
            tempWeekBasket.RetrieveProducts(_context);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == tempWeekBasket.Consumer.Id);

            if (validatedWeekBasket == null)
            {
                //First validation of the week
                validatedWeekBasket = new ValidatedWeekBasket();
                validatedWeekBasket.Products = new List<BillEntry>();
                validatedWeekBasket.Consumer = tempWeekBasket.Consumer;
                _context.Add(validatedWeekBasket);
            }
            else
            {
                validatedWeekBasket.RetrieveProducts(_context);
            }
            //LOCK to prevent multi insert at this momment
            if (tempWeekBasket.Products.Any())
            {
                List<BillEntry> rejectedEntries = new List<BillEntry>();
                //Sauvegarde des produits déja validés
                List<BillEntry> previousBillEntries = validatedWeekBasket.Products;
                //On met le panier validé dans le même état que le temporaire
                validatedWeekBasket.Products = new List<BillEntry>();
                foreach (BillEntry billEntry in tempWeekBasket.Products.ToList())
                {
                    validatedWeekBasket.Products.Add(billEntry.Clone());
                }

                //Gestion de la suppression et du changement de quantité sur des billEntry existantes
                foreach (BillEntry prevEntry in previousBillEntries)
                {
                    BillEntry newEntry = validatedWeekBasket.Products.FirstOrDefault(x => x.ProductId == prevEntry.ProductId);
                    ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).First(x => x.ProductId == prevEntry.ProductId && x.AdherentStolon.StolonId == GetCurrentStolon().Id);
                    
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
                            stepStock = (productStock.RemainingStock / productStock.Product.QuantityStep) * 1000.0M;
                        }
                        if (stepStock < qtyDiff)
                        {
                            //Stock insuffisant, on supprime la nouvelle ligne et on garde l'ancienne
                            validatedWeekBasket.Products.Remove(newEntry);
                            validatedWeekBasket.Products.Add(prevEntry);
                            rejectedEntries.Add(newEntry);
                        }
                        else
                        {
                            UpdateProductStock(productStock, -qtyDiff);
                        }
                    }
                }

                //Gestion de l'ajout de produits
                foreach (BillEntry newEntry in validatedWeekBasket.Products.ToList())
                {
                    BillEntry prevEntry = previousBillEntries.FirstOrDefault(x => x.ProductId == newEntry.ProductId);

                    if (prevEntry == null)
                    {
                        //Nouveau produit
                        ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).First(x => x.ProductId == newEntry.ProductId && x.AdherentStolon.StolonId == GetCurrentStolon().Id);

                        decimal stepStock = productStock.RemainingStock;
                        if (productStock.Product.Type != Product.SellType.Piece)
                        {
                            stepStock = (productStock.RemainingStock / productStock.Product.QuantityStep) * 1000.0M;
                        }
                        if (newEntry.Quantity <= stepStock)
                        {
                            //product.RemainingStock -= newEntry.Quantity;
                            UpdateProductStock(productStock, -newEntry.Quantity);
                        }
                        else
                        {
                            validatedWeekBasket.Products.Remove(newEntry);
                            rejectedEntries.Add(newEntry);
                        }
                    }
                }

                tempWeekBasket.Products = new List<BillEntry>();
                //On met le panier temporaire dans le même état que le validé
                foreach (BillEntry entry in validatedWeekBasket.Products)
                {
                    tempWeekBasket.Products.Add(entry.Clone());
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
                ValidationSummaryViewModel validationSummaryViewModel = new ValidationSummaryViewModel(validatedWeekBasket, rejectedEntries) { Total = GetBasketPrice(validatedWeekBasket) };
                Services.AuthMessageSender.SendEmail(validatedWeekBasket.Consumer.Email, validatedWeekBasket.Consumer.Name, subject, base.RenderPartialViewToString("Templates/ValidatedBasketTemplate", validationSummaryViewModel));
                //Return view
                return View("ValidateBasket", validationSummaryViewModel);
            }
            else
            {
                //On annule tout le contenu du panier
                foreach (BillEntry entry in validatedWeekBasket.Products)
                {
                    ProductStockStolon productStock = _context.ProductsStocks.Include(x => x.Product).Include(x => x.AdherentStolon).First(x => x.ProductId == entry.ProductId && x.AdherentStolon.StolonId == GetCurrentStolon().Id);
                    UpdateProductStock(productStock, entry.Quantity);
                    //entry.Product.RemainingStock += entry.Quantity;
                }
                _context.Remove(tempWeekBasket);
                _context.Remove(validatedWeekBasket);
                _context.SaveChanges();

                //Il ne commande rien du tout
                //On lui signale
                Services.AuthMessageSender.SendEmail(validatedWeekBasket.Consumer.Email, validatedWeekBasket.Consumer.Name, "Panier de la semaine annulé", base.RenderPartialViewToString("ValidateBasket", null));
            }
            return View("ValidateBasket");
        }

        //Calcul du prix total d'un panier
        private Decimal GetBasketPrice(IWeekBasket basket)
        {
            if (basket == null)
            {
                return 0;
            }
            Decimal price = 0;
            foreach (BillEntry entry in basket.Products)
            {
                Product product = _context.Products.First(x => x.Id == entry.ProductId);
                price += (product.UnitPrice * entry.Quantity);
            }
            return price;
        }

        private bool IsBasketValidated(TempWeekBasket tmpBasket)
        {
            ValidatedWeekBasket validatedBasket = _context.ValidatedWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == tmpBasket.Consumer.Id);

            if (validatedBasket == null)
            {
                return false;
            }
            if (validatedBasket.Products.Count != tmpBasket.Products.Count)
            {
                return false;
            }
            foreach (BillEntry billEntry in tmpBasket.Products.ToList())
            {
                BillEntry validatedEntry = validatedBasket.Products.FirstOrDefault(x => x.ProductId == billEntry.ProductId);

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
