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
            var products = _context.Products.Include(m => m.Familly).Include(m => m.Familly.Type).Where(x => x.Producer == producer);
            ProductsViewModel vm = new ProductsViewModel(GetActiveAdherentStolon(), products, GetCurrentStolon());
            return View(vm);
        }
        
        [HttpGet, ActionName("ProducerProducts"), Route("api/producerProducts")]
        public string JsonProducerProducts()
        {
            if (!AuthorizedProducer())
                return "401";

            Adherent producer = GetCurrentAdherentSync() as Adherent;
            List<ProductViewModel> vmProducts = new List<ProductViewModel>();
            var products = _context.Products.Include(m => m.Familly).Include(m => m.Familly.Type).Where(x => x.Producer == producer).ToList();
            foreach (var product in products)
            {
                int orderedQty = 0;
                List<BillEntry> billEntries = new List<BillEntry>();
                foreach (var validateWeekBasket in _context.ValidatedWeekBaskets.Include(x => x.Products))
                {
                    validateWeekBasket.Products.Where(x => x.ProductId == product.Id).ToList().ForEach(x => orderedQty += x.Quantity);
                }
                vmProducts.Add(new ProductViewModel(GetActiveAdherentStolon(), product, orderedQty));
            }
            return JsonConvert.SerializeObject(vmProducts, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
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

            Product product = id == null ? new Product() : _context.Products.Include(x => x.Familly).First(x => x.Id == id);
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
                //Set Producer (si ça retourne null, c'est que c'est pas un producteur qui est logger, alors on est dans la merde)
                Adherent producer = await GetCurrentAdherentAsync() as Adherent;
                vmProduct.Product.Producer = producer;
                //On s'occupe des images du produit
                if (!String.IsNullOrWhiteSpace(vmProduct.MainPictureLight))
                {
                    string pictureName = Guid.NewGuid().ToString() + ".png";
                    Configurations.UploadImageFile(_environment, vmProduct.MainPictureLight, Configurations.ProductsStockagePathLight, pictureName);
                    Configurations.UploadImageFile(_environment, vmProduct.MainPictureHeavy, Configurations.ProductsStockagePathHeavy, pictureName);
                    if (!vmProduct.IsNew)
                    {
                        //Replace
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.ProductsStockagePathLight, vmProduct.Product.Pictures[0]));
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.ProductsStockagePathHeavy, vmProduct.Product.Pictures[0]));
                        vmProduct.Product.Pictures[0] = pictureName;
                    }
                    else
                    {
                        //Add
                        vmProduct.Product.Pictures.Add(pictureName);
                    }
                }
                if (!String.IsNullOrWhiteSpace(vmProduct.Picture2Light))
                {
                    string pictureName = Guid.NewGuid().ToString() + ".png";
                    Configurations.UploadImageFile(_environment, vmProduct.Picture2Light, Configurations.ProductsStockagePathLight, pictureName);
                    Configurations.UploadImageFile(_environment, vmProduct.Picture2Heavy, Configurations.ProductsStockagePathHeavy, pictureName);
                    if (!vmProduct.IsNew)
                    {
                        //Replace 
                        //Dessous code foireux
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.ProductsStockagePathLight, vmProduct.Product.Pictures[1]));
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.ProductsStockagePathHeavy, vmProduct.Product.Pictures[1]));
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
                    string pictureName = Guid.NewGuid().ToString() + ".png";
                    Configurations.UploadImageFile(_environment, vmProduct.Picture3Light, Configurations.ProductsStockagePathLight, pictureName);
                    Configurations.UploadImageFile(_environment, vmProduct.Picture3Heavy, Configurations.ProductsStockagePathHeavy, pictureName);
                    if (!vmProduct.IsNew)
                    {
                        //Replace
                        //Dessous code foireux
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.ProductsStockagePathLight, vmProduct.Product.Pictures[2]));
                        System.IO.File.Delete(Path.Combine(_environment.WebRootPath, Configurations.ProductsStockagePathHeavy, vmProduct.Product.Pictures[2]));
                        vmProduct.Product.Pictures[2] = pictureName;
                    }
                    else
                    {
                        //Add
                        vmProduct.Product.Pictures.Add(pictureName);
                    }
                }


                /*
                //OLD CODE
                int cpt = 0;
                foreach (IFormFile uploadFile in new List<IFormFile>() { vmProduct.UploadFile1, vmProduct.UploadFile2, vmProduct.UploadFile3 })
                {
                    if (uploadFile != null)
                    {
                        //Image uploading
                        string fileName = await Configurations.UploadImageFile(_environment, uploadFile, Configurations.ProductsStockagePath);
                        if(!vmProduct.IsNew && vmProduct.Product.Pictures.Count > cpt)
                        {
                            //Replace
                            vmProduct.Product.Pictures[cpt] = fileName;
                        }
                        else
                        {
                            //Add
                            vmProduct.Product.Pictures.Add(fileName);
                        }
                    }
                    cpt++;
                }*/

                if (vmProduct.IsNew)
                {
                    vmProduct.Product.Id = Guid.NewGuid();
                    _context.Products.Add(vmProduct.Product);
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

        // GET: ProductsManagement/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(Guid? id)
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

        // POST: ProductsManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            Product product = _context.Products.FirstOrDefault(x => x.Id == id);
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult EnableForAllStolon(Guid? productId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var productStock in _context.ProductsStocks.Where(x => x.ProductId == productId))
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

            foreach (var product in _context.ProductsStocks.Include(x => x.AdherentStolon).Where(x => x.AdherentStolon.AdherentId == GetCurrentAdherentSync().Id && x.State == Product.ProductState.Stock))
            {
                product.State = Product.ProductState.Enabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult EnableAllStockProductForStolon(Guid? stolonId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Include(x => x.AdherentStolon).Where(x => x.AdherentStolon.StolonId == stolonId &&  x.State == Product.ProductState.Stock))
            {
                product.State = Product.ProductState.Enabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult DisableAllProduct()
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Include(x => x.AdherentStolon).Where(x => x.AdherentStolon.AdherentId == GetCurrentAdherentSync().Id && x.State != Product.ProductState.Disabled))
            {
                product.State = Product.ProductState.Disabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public IActionResult EnableForSpecified(Guid? productStockId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            _context.ProductsStocks.First(x => x.Id == productStockId).State = Product.ProductState.Enabled;
            _context.SaveChanges();
            return RedirectToAction("Index");

        }
        public IActionResult DisableForSpecified(Guid? productStockId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            _context.ProductsStocks.First(x => x.Id == productStockId).State = Product.ProductState.Disabled;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        
        public IActionResult EnableAllStockProductForSpecified(Guid? adherentStolonId)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            foreach (var product in _context.ProductsStocks.Where(x => x.AdherentStolonId == adherentStolonId && x.State == Product.ProductState.Stock))
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

            foreach (var product in _context.ProductsStocks.Where(x => x.AdherentStolonId == adherentStolonId && x.State != Product.ProductState.Disabled))
            {
                product.State = Product.ProductState.Disabled;
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        
        [HttpPost, ActionName("ChangeStock")]
        public IActionResult ChangeStock(Guid productStockId, decimal newStock)
        {
            if (!AuthorizedProducer())
                return Unauthorized();

            _context.ProductsStocks.First(x => x.Id == productStockId).WeekStock = newStock;
            _context.ProductsStocks.First(x => x.Id == productStockId).RemainingStock = newStock;
            _context.SaveChanges();
            return Ok();
        }
        
        [HttpPost, ActionName("ChangeCurrentStock")]
        public string ChangeCurrentStock(Guid productStockId, decimal newStock)
        {
            if (!AuthorizedProducer())
                return "401";
            var productStock = _context.ProductsStocks.First(x => x.Id == productStockId);
            productStock.RemainingStock = newStock;
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
            {
                return StatusCode(400);
            }
            var products = _context.Products.Where(x => x.Familly.Type.Id == categoryId);
            foreach (Product product in products)
            {
                product.Familly = null;
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
            var products = _context.Products.Where(x => x.Familly.Id == familyId);
            foreach (Product product in products)
            {
                product.Familly = null;
            }
            _context.ProductFamillys.Remove(family);
            _context.SaveChanges();
            return Ok();
        }


    }
}
