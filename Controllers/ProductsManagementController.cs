using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Stolons.ViewModels.ProductsManagement;
using Microsoft.AspNetCore.Authorization;
using Stolons.Models.Users;

namespace Stolons.Controllers
{
    public class ProductsManagementController : BaseController
    {
        public ProductsManagementController(ApplicationDbContext context, IHostingEnvironment environment,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IServiceProvider serviceProvider) : base(serviceProvider, userManager, context, environment, signInManager)
        {

        }

        // GET: ProductsManagement
        public async Task<IActionResult> Index()
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            Adherent producer = await GetCurrentAdherentAsync() as Adherent;
            producer = _context.Adherents
                .Include(x => x.Products).ThenInclude(x => x.ProductStocks).ThenInclude(x => x.AdherentStolon).ThenInclude(x => x.Stolon)
                .Include(x => x.Products).ThenInclude(x=>x.Familly)
                .Include(x => x.Products).ThenInclude(x => x.Familly.Type)
                .Include(x => x.AdherentStolons).AsNoTracking().FirstOrDefault(x => x.Id == producer.Id);
            producer.AdherentStolons.ForEach(adherentStolon => adherentStolon.Stolon = _context.Stolons.First(stolon => stolon.Id == adherentStolon.StolonId));
            var products = _context.Products.Include(x=>x.ProductStocks).Include(m => m.Familly).Include(m => m.Familly.Type).Where(x => x.Producer == producer).OrderBy(x=>x.Name).ToList();
            foreach (var prod in products)
            {
                foreach (var stock in prod.ProductStocks)
                {
                    stock.Product = prod;
                    stock.BillEntries = _context.BillEntrys.Where(x => x.ProductStockId == stock.Id).ToList();
                }
            }
            products.ForEach(prod => prod.ProductStocks.ForEach(stock => stock.Product = prod));
            ProductsViewModel vm = new ProductsViewModel(GetActiveAdherentStolon(), products, producer);
            return View(vm);
        }

        [HttpGet, ActionName("ProducerProducts"), Route("api/producerProducts")]
        public IActionResult JsonProducerProducts()
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            Adherent producer = GetCurrentAdherentSync() as Adherent;
            List<ProductStockViewModel> vmProductsStock = new List<ProductStockViewModel>();
            var productsStock = _context.ProductsStocks
		.Include(x=>x.AdherentStolon)
		.ThenInclude(x=>x.Stolon)
		.Include(x=>x.Product)
		.ThenInclude(m => m.Familly)
		.ThenInclude(m => m.Type)
		.AsNoTracking()
		.Where(x => x.AdherentStolon.AdherentId== producer.Id).ToList();
           
            foreach (var productStock in productsStock)
            {
                int orderedQty = 0;
                foreach (var validateWeekBasket in _context.ValidatedWeekBaskets.Include(x=>x.AdherentStolon).ThenInclude(x=>x.Adherent).Include(x => x.BillEntries).AsNoTracking().ToList())
                {
                    validateWeekBasket.BillEntries.Where(x => x.ProductStockId == productStock.Id).ToList().ForEach(x => orderedQty += x.Quantity);
                }
                vmProductsStock.Add(new ProductStockViewModel(GetActiveAdherentStolon(), productStock, orderedQty));
            }
            return Json(vmProductsStock);
        }

        // GET: ProductsManagement/Details/5
        public IActionResult Details(Guid? id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            if (id == null)
            {
                return NotFound();
            }

            Product product = _context.Products.FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: ProductsManagement/Create
        public IActionResult Manage(Guid? id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            Product product = id == null ? new Product() { Familly = _context.ProductFamillys.First(x=>x.Id ==  Configurations.DefaultFamily.Id) } : _context.Products.Include(x => x.Familly).First(x => x.Id == id);
            return View(new ProductEditionViewModel(GetActiveAdherentStolon(), product, _context, id == null));

        }

        // POST: ProductsManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ProductEditionViewModel vmProduct)
        {
            if (!AuthorizedProducer())
                return Unauthorized();
            if (ModelState.IsValid)
            {
                //Set Labels
                vmProduct.Product.SetLabels(vmProduct.SelectedLabels);
                //Set Product familly (si ça retourne null c'est que la famille selectionnée n'existe pas, alors on est dans la merde)
                vmProduct.Product.Familly = _context.ProductFamillys.FirstOrDefault(x => x.FamillyName == vmProduct.FamillyName);
                vmProduct.Product.FamillyId = vmProduct.Product.Familly.Id;
                //Set Producer (si ça retourne null, c'est que c'est pas un producteur qui est logger, alors on est dans la merde)
                Adherent producer = await GetCurrentAdherentAsync() as Adherent;
                vmProduct.Product.Producer = producer;
                vmProduct.Product.ProducerId = producer.Id;
                //On s'occupe des images du produit
                if (!String.IsNullOrWhiteSpace(vmProduct.MainPictureLight))
                {
                    string pictureName = Guid.NewGuid().ToString() + ".jpg";
                    _environment.UploadBase64Image( vmProduct.MainPictureLight, Configurations.ProductsStockagePathLight, pictureName);
                    _environment.UploadBase64Image( vmProduct.MainPictureHeavy, Configurations.ProductsStockagePathHeavy, pictureName);
                    if (vmProduct.IsNew || vmProduct.Product.Pictures.Count == 0)
                    {
                        //Add
                        vmProduct.Product.Pictures.Add(pictureName);
                    }
                    else
                    {
                        //Replace
                        _environment.DeleteFile(Configurations.ProductsStockagePathLight, vmProduct.Product.Pictures[0]);
                        _environment.DeleteFile(Configurations.ProductsStockagePathHeavy, vmProduct.Product.Pictures[0]);
                        vmProduct.Product.Pictures[0] = pictureName;
                    }
                }
                if (!String.IsNullOrWhiteSpace(vmProduct.Picture2Light))
                {
                    string pictureName = Guid.NewGuid().ToString() + ".jpg";
                    _environment.UploadBase64Image( vmProduct.MainPictureLight, Configurations.ProductsStockagePathLight, pictureName);
                    _environment.UploadBase64Image( vmProduct.MainPictureHeavy, Configurations.ProductsStockagePathHeavy, pictureName);
                    if (!vmProduct.IsNew)
                    {
                        _environment.DeleteFile(Configurations.ProductsStockagePathLight, vmProduct.Product.Pictures[1]);
                        _environment.DeleteFile(Configurations.ProductsStockagePathHeavy, vmProduct.Product.Pictures[1]);
                        vmProduct.Product.Pictures[1] = pictureName;
                    }
                    else
                    {
                        //Add
                        vmProduct.Product.Pictures.Add(pictureName);
                    }
                }
                if (!String.IsNullOrWhiteSpace(vmProduct.Picture3Light))
                {
                    string pictureName = Guid.NewGuid().ToString() + ".jpg";
                    _environment.UploadBase64Image( vmProduct.MainPictureLight, Configurations.ProductsStockagePathLight, pictureName);
                    _environment.UploadBase64Image( vmProduct.MainPictureHeavy, Configurations.ProductsStockagePathHeavy, pictureName);
                    if (!vmProduct.IsNew)
                    {
                        _environment.DeleteFile(Configurations.ProductsStockagePathLight, vmProduct.Product.Pictures[2]);
                        _environment.DeleteFile(Configurations.ProductsStockagePathHeavy, vmProduct.Product.Pictures[2]);
                        vmProduct.Product.Pictures[2] = pictureName;
                    }
                    else
                    {
                        //Add
                        vmProduct.Product.Pictures.Add(pictureName);
                    }
                }

                if (vmProduct.IsNew)
                {
                    _context.Products.Add(vmProduct.Product);
                    //Add it to all Stolon
                    foreach (var adhrentStolon in _context.AdherentStolons.Where(x => x.AdherentId == vmProduct.Product.ProducerId && x.IsProducer).ToList())
                    {
                        _context.ProductsStocks.Add(new ProductStockStolon(vmProduct.Product.Id, adhrentStolon.Id));
                    }
                }
                else
                {
                    _context.Products.Update(vmProduct.Product);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            vmProduct.RefreshTypes(_context);
            return View(vmProduct);
        }
        
        public IActionResult Delete(Guid id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            Product product = _context.Products.FirstOrDefault(x => x.Id == id);
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult Archive(Guid id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            Product product = _context.Products.Include(x=>x.ProductStocks).FirstOrDefault(x => x.Id == id);
            product.IsArchive = true;
            product.ProductStocks.ToList().ForEach(x => x.State = Product.ProductState.Disabled);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UnArchive(Guid id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            Product product = _context.Products.FirstOrDefault(x => x.Id == id);
            product.IsArchive = false;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult EnableForAllStolon(Guid? productId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var productStock in _context.ProductsStocks.Where(x => x.ProductId == productId).ToList())
            {
                productStock.State = Product.ProductState.Enabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");

        }
        
        public IActionResult EnableAllStockProduct()
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Include(x => x.AdherentStolon).Where(x => x.AdherentStolon.AdherentId == GetCurrentAdherentSync().Id && x.State == Product.ProductState.Stock).ToList())
            {
                product.State = Product.ProductState.Enabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult EnableAllStockProductForStolon(Guid? adherentStolonId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Include(x => x.AdherentStolon).Where(x => x.AdherentStolon.Id == adherentStolonId &&  x.State == Product.ProductState.Stock).ToList())
            {
                product.State = Product.ProductState.Enabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult DisableAllProductForStolon(Guid adherentStolonId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Include(x => x.AdherentStolon).Where(x => x.AdherentStolonId == adherentStolonId && x.State != Product.ProductState.Disabled ).ToList())
            {
                product.State = Product.ProductState.Disabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult Enable(Guid? id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            ProductStockStolon productStock = _context.ProductsStocks.FirstOrDefault(x => x.Id == id);
            if (productStock == null)
                return NotFound();
            productStock.State = Product.ProductState.Enabled;
            _context.SaveChanges();
            return RedirectToAction("Index");

        }
        public IActionResult Disable(Guid? id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            ProductStockStolon productStock = _context.ProductsStocks.FirstOrDefault(x => x.Id == id);
            if (productStock == null)
                return NotFound();
            productStock.State = Product.ProductState.Disabled;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        
        public IActionResult EnableAllStockProductForSpecified(Guid? adherentStolonId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Where(x => x.AdherentStolonId == adherentStolonId && x.State == Product.ProductState.Stock).ToList())
            {
                product.State = Product.ProductState.Enabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult DisableAllProductForSpecified(Guid? adherentStolonId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Where(x => x.AdherentStolonId == adherentStolonId && x.State != Product.ProductState.Disabled).ToList())
            {
                product.State = Product.ProductState.Disabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        
        [HttpPost, ActionName("ChangeStock")]
        public IActionResult ChangeStock(Guid id, decimal stockDiff)
        {
            if (!AuthorizedProducer())
                return Unauthorized();
	    var productStock = _context.ProductsStocks.First(x => x.Id == id);
            productStock.WeekStock = productStock.WeekStock + stockDiff;
	    productStock.RemainingStock = productStock.WeekStock;
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost, ActionName("ChangeCurrentStock")]
        public string ChangeCurrentStock(Guid id, decimal stockDiff)
        {
            if (!AuthorizedProducer())
                return "401";
            var productStock = _context.ProductsStocks.First(x => x.Id == id);
	    //TODO verify that remaining stock >= 0
	    if (productStock.RemainingStock + stockDiff < 0)
	    {
		return "Erreur: impossible de baisser le stock (stock restant: " + productStock.RemainingStock+ ")";
	    }
            productStock.RemainingStock = productStock.RemainingStock + stockDiff;
            _context.SaveChanges();
            return "ok";
        }
        
        [HttpGet, ActionName("ManageFamilies")]
        public IActionResult ManageFamilies()
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            return View();
        }
        
        [HttpPost, ActionName("CreateCategory")]
        public IActionResult CreateCategory(string categoryName)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            var productCategory = new ProductType(categoryName);
            _context.ProductTypes.Add(productCategory);
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost, ActionName("CreateFamily")]
        public IActionResult CreateFamily(Guid categoryId, string familyName)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            var productCategory = _context.ProductTypes.FirstOrDefault(x => x.Id == categoryId);
            if (productCategory == null)
            {
                return StatusCode(400);
            }
            var productFamily = new ProductFamilly(productCategory, familyName);
            _context.ProductFamillys.Add(productFamily);
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost, ActionName("RenameCategory")]
        public IActionResult RenameCategory(Guid categoryId, string newCategoryName)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            var category = _context.ProductTypes.FirstOrDefault(x => x.Id == categoryId);
            if (category == null)
            {
                return StatusCode(400);
            }
            category.Name = newCategoryName;
            _context.SaveChanges();
            return Ok();
        }

        
        [HttpPost, ActionName("RenameFamily")]
        public IActionResult RenameFamily(Guid familyId, string newFamilyName)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            var family = _context.ProductFamillys.FirstOrDefault(x => x.Id == familyId);
            if (family == null)
            {
                return StatusCode(400);
            }
            family.FamillyName = newFamilyName;
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost, ActionName("UpdateCategoryPicture")]
        public async Task<IActionResult> UpdateCategoryPicture(Guid categoryId, IFormFile picture)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            var category = _context.ProductTypes.FirstOrDefault(x => x.Id == categoryId);
            if (category == null)
            {
                return StatusCode(400);
            }
            string fileName = await Configurations.UploadImageFile(_environment, picture, Configurations.ProductsTypeAndFamillyIconsStockagesPath);
            category.Image = fileName;
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost, ActionName("UpdateFamilyPicture")]
        public async Task<IActionResult> UpdateFamilyPicture(Guid familyId, IFormFile picture)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();

            var family = _context.ProductFamillys.FirstOrDefault(x => x.Id == familyId);
            if (family == null)
            {
                return StatusCode(403);
            }
            string fileName = await Configurations.UploadImageFile(_environment, picture, Configurations.ProductsTypeAndFamillyIconsStockagesPath);
            family.Image = fileName;
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost, ActionName("DeleteCategory")]
        public IActionResult DeleteCategory(Guid categoryId)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            var category = _context.ProductTypes.Include(x => x.ProductFamilly).FirstOrDefault(x => x.Id == categoryId);
            if (category == null)
                return StatusCode(400);
            if (!category.CanBeRemoved)
                return StatusCode(401, "Cette catégorie ne peut être supprimé");
            var products = _context.Products.Where(x => x.Familly.Type.Id == categoryId).ToList();
            var defaultFamilly = _context.ProductFamillys.First(x => x.Id == Configurations.DefaultFamily.Id);
            foreach (Product product in products)
            {
                product.Familly = defaultFamilly;
                _context.Update(product);
            }
            foreach (ProductFamilly family in category.ProductFamilly)
            {
                _context.ProductFamillys.Remove(family);
            }
            _context.ProductTypes.Remove(category);
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost, ActionName("DeleteFamily")]
        public IActionResult DeleteFamily(Guid familyId)
        {
            if (!AuthorizedWebAdmin())
                return Unauthorized();
            var family = _context.ProductFamillys.FirstOrDefault(x => x.Id == familyId);
            if (family == null)
            {
                return StatusCode(400);
            }
            if (!family.CanBeRemoved)
                return StatusCode(401, "Cette famille ne peut être supprimé");
            var products = _context.Products.Where(x => x.Familly.Id == familyId).ToList();
            var defaultFamilly = _context.ProductFamillys.First(x => x.Id == Configurations.DefaultFamily.Id);
            foreach (Product product in products)
            {
                product.Familly = defaultFamilly;
                _context.Update(product);
            }
            _context.ProductFamillys.Remove(family);
            _context.SaveChanges();
            return Ok();
        }


    }
}
