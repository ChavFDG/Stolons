using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.ProductsManagement
{
    public class ProductStockViewModel : BaseViewModel
    {

        public ProductStockStolon ProductStock { get; set; }

        public string OrderedQuantityString;

        public ProductStockViewModel()
        {
        }

        public ProductStockViewModel(AdherentStolon activeAdherentStolon, ProductStockStolon productStock, int orderedQty)
        {
            ProductStock = productStock;
            OrderedQuantityString = productStock.Product.GetQuantityString(orderedQty);
            ActiveAdherentStolon = activeAdherentStolon;
        }
    }
}
