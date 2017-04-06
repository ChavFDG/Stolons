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
    public class WeekBasketViewModel : BaseViewModel
    {

        public AdherentStolon AdherentStolon { get; set; }

        public List<ProductStockStolon> ProductsStocks { get; set; }

        public List<ProductType> ProductTypes { get; set; }

        public TempWeekBasket TempWeekBasket { get; set; }
        public ValidatedWeekBasket ValidatedWeekBasket { get; set; }

        public WeekBasketViewModel()
        {
        }

        public WeekBasketViewModel(AdherentStolon activeAdherentStolon,AdherentStolon adherentStolon, TempWeekBasket tempWeekBasket, ValidatedWeekBasket validatedWeekBasket, ApplicationDbContext context)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            TempWeekBasket = tempWeekBasket;
            ValidatedWeekBasket = validatedWeekBasket;
            AdherentStolon = adherentStolon;
            ProductsStocks = context.ProductsStocks.Include(x => x.Product).Include(x=>x.AdherentStolon).Where(x => x.State == Product.ProductState.Enabled).Where(x=>x.Product.IsAvailable).ToList();
            ProductTypes = context.ProductTypes.Include(x => x.ProductFamilly).ToList();
        }
    }
}
