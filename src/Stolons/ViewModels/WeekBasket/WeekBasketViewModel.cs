using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasket
{
    public class WeekBasketViewModel
    {
        public Consumer Consumer { get; set; }

        public List<Product> Products { get; set; }

        public List<ProductType> ProductTypes { get; set;}

        public TempWeekBasket TempWeekBasket { get; set; }
        public ValidatedWeekBasket ValidatedWeekBasket { get; set; }

        public WeekBasketViewModel() 
        {
        }

        public WeekBasketViewModel(Consumer consumer, TempWeekBasket tempWeekBasket,ValidatedWeekBasket validatedWeekBasket, ApplicationDbContext context)
        {
            TempWeekBasket = tempWeekBasket;
            ValidatedWeekBasket = validatedWeekBasket;
            Consumer = consumer;
            Products = context.Products.Include(x=>x.Producer).Where(x => x.State == Product.ProductState.Enabled).ToList();
            ProductTypes = context.ProductTypes.Include(x => x.ProductFamilly).ToList();
        }
    }
}
