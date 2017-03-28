using Microsoft.EntityFrameworkCore;
using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasket
{
    public class WeekBasketViewModel
    {

        public AdherentStolon UserStolon { get; set; }

        public List<Product> Products { get; set; }

        public List<ProductType> ProductTypes { get; set; }

        public TempWeekBasket TempWeekBasket { get; set; }
        public ValidatedWeekBasket ValidatedWeekBasket { get; set; }

        public WeekBasketViewModel()
        {
        }

        public WeekBasketViewModel(AdherentStolon userStolon, TempWeekBasket tempWeekBasket, ValidatedWeekBasket validatedWeekBasket, ApplicationDbContext context)
        {
            TempWeekBasket = tempWeekBasket;
            ValidatedWeekBasket = validatedWeekBasket;
            UserStolon = userStolon;
            Products = context.Products.Include(x => x.Producer).Where(x => x.State == Product.ProductState.Enabled).ToList();
            ProductTypes = context.ProductTypes.Include(x => x.ProductFamilly).ToList();
        }
    }
}
