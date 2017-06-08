using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.ProductsManagement
{
    public class ProductEditionViewModel : BaseViewModel
    {
        public bool IsNew { get; set; }

        public string MainPictureLight { get; set; }
        public string MainPictureHeavy { get; set; }
        public string Picture2Light { get; set; }
        public string Picture2Heavy { get; set; }
        public string Picture3Light { get; set; }
        public string Picture3Heavy { get; set; }

        public string[] SelectedLabels { get; set; }

        public string FamillyName { get; set; }

        public Product Product { get; set; }

        public List<ProductType> ProductTypes { get; set; }
        public ProductEditionViewModel()
        {
        }

        public ProductEditionViewModel(AdherentStolon activeAdherentStolon, Product product, ApplicationDbContext context, bool isNew)
        {
            ActiveAdherentStolon = activeAdherentStolon;
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
