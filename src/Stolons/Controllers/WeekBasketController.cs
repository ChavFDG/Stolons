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

namespace Stolons.Controllers
{
    [Authorize]
    public class WeekBasketController : BaseController
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IHostingEnvironment _environment;

        public WeekBasketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment environment,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: WeekBasket/Index/id
        public async Task<IActionResult> Index()
        {
            var appUser = await GetCurrentUserAsync(_userManager);
            Consumer consumer = _context.Consumers.FirstOrDefault(x => x.Email == appUser.Email);
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
            return View(new WeekBasketViewModel(consumer, tempWeekBasket, validatedWeekBasket, _context));
        }

        [AllowAnonymous]
        [HttpGet, ActionName("Products"), Route("api/products")]
        public string JsonProducts()
        {
            var Products = _context.Products.Include(x => x.Producer).Include(x => x.Familly).Where(x => x.State == Product.ProductState.Enabled).ToList();
            return JsonConvert.SerializeObject(Products, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        [AllowAnonymous]
        [HttpGet, ActionName("PublicProducts"), Route("api/publicProducts")]
        public string JsonPublicProducts()
        {
            var Products = _context.Products.Include(x => x.Producer).Include(x => x.Familly).ToList();
            return JsonConvert.SerializeObject(Products, Formatting.Indented, new JsonSerializerSettings()
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
            var appUser = await GetCurrentUserAsync(_userManager);
            Consumer consumer = _context.Consumers.FirstOrDefault(x => x.Email == appUser.Email);
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
                tempWeekBasket.Products = new System.Collections.Generic.List<BillEntry>();
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
            var appUser = await GetCurrentUserAsync(_userManager);
            Consumer consumer = _context.Consumers.FirstOrDefault(x => x.Email == appUser.Email);
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
            billEntry.Product = _context.Products.First(x => x.Id.ToString() == productId);
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

        private TempWeekBasket AddProductQuantity(string weekBasketId, string productId, int quantity)
        {
            TempWeekBasket tempWeekBasket = _context.TempsWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).First(x => x.Id.ToString() == weekBasketId);
            tempWeekBasket.RetrieveProducts(_context);
            ValidatedWeekBasket validatedWeekBasket = _context.ValidatedWeekBaskets.Include(x => x.Consumer).Include(x => x.Products).FirstOrDefault(x => x.Consumer.Id == tempWeekBasket.Consumer.Id);

            int validatedQuantity = 0;
            if (validatedWeekBasket != null)
            {
                BillEntry validatedEntry = validatedWeekBasket.Products.FirstOrDefault(x => x.ProductId.ToString() == productId);

                if (validatedEntry != null)
                {
                    validatedQuantity = validatedEntry.Quantity;
                }
            }
            BillEntry billEntry = tempWeekBasket.Products.FirstOrDefault(x => x.ProductId.ToString() == productId);
            Product product = _context.Products.FirstOrDefault(x => x.Id.ToString() == productId);

            float stepStock = product.RemainingStock;
            if (product.Type != Product.SellType.Piece)
            {
                stepStock = (product.RemainingStock * 1000) / product.QuantityStep;
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
        private void updateProductStock(Product product, int qty)
        {
            if (product.Type == Product.SellType.Piece)
            {
                product.RemainingStock += qty;
            }
            else
            {
                product.RemainingStock += ((float)qty * product.QuantityStep) / 1000;
            }
        }

        [HttpPost, ActionName("ValidateBasket")]
        public IActionResult ValidateBasket(string basketId)
        {
            if (Configurations.Mode == ApplicationConfig.Modes.DeliveryAndStockUpdate)
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
                    Product product = _context.Products.First(x => x.Id == prevEntry.ProductId);

                    if (newEntry == null)
                    {
                        //produit supprimé du panier
                        updateProductStock(product, prevEntry.Quantity);
                        //product.RemainingStock += prevEntry.Quantity;
                    }
                    else
                    {
                        int qtyDiff = newEntry.Quantity - prevEntry.Quantity;
                        float stepStock = product.RemainingStock;
                        if (product.Type != Product.SellType.Piece)
                        {
                            stepStock = (product.RemainingStock / product.QuantityStep) * 1000;
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
                            updateProductStock(product, -qtyDiff);
                            //product.RemainingStock -= qtyDiff;
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
                        Product product = _context.Products.First(x => x.Id == newEntry.ProductId);
                        float stepStock = product.RemainingStock;
                        if (product.Type != Product.SellType.Piece)
                        {
                            stepStock = (product.RemainingStock / product.QuantityStep) * 1000;
                        }
                        if (newEntry.Quantity <= stepStock)
                        {
                            //product.RemainingStock -= newEntry.Quantity;
                            updateProductStock(product, -newEntry.Quantity);
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
                    Product product = _context.Products.First(x => x.Id == entry.ProductId);
                    updateProductStock(entry.Product, entry.Quantity);
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
        private float GetBasketPrice(IWeekBasket basket)
        {
            if (basket == null)
            {
                return 0;
            }
            float price = 0;
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
