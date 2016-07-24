using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.ProductsManagement
{
    public class ProductViewModel
    {

	public Product Product { get; set; }

	public string OrderedQuantityString;

	public ProductViewModel()
        {
        }

        public ProductViewModel(Product product, int orderedQty)
        {
	    Product = product;
	    OrderedQuantityString = product.GetQuantityString(orderedQty);
	}
    }
}
