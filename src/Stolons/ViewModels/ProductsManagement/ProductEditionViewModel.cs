using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.ProductsManagement
{
    public class ProductEditionViewModel
    {
        public bool IsNew { get; set; }


        public string[] SelectedLabels { get; set; }

        public string FamillyName { get; set; }

        public Product Product { get; set; }

        public List<ProductType> ProductTypes { get; set; }

        public IFormFile UploadFile1 { get; set; }
        public IFormFile UploadFile2 { get; set; }
        public IFormFile UploadFile3 { get; set; }

        public ProductEditionViewModel()
        {
        }

        public ProductEditionViewModel(Product product, ApplicationDbContext context, bool isNew)
        {
            Product = product;
            IsNew = isNew;
            RefreshTypes(context);
        }

        public void RefreshTypes(ApplicationDbContext context)
        {
            ProductTypes = context.ProductTypes.Include(x => x.ProductFamilly).ToList();
            SelectedLabels = Product.Labels.Select(s => s.ToString()).ToArray();
        }
    }
}
