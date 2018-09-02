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
        public List<ProductType> ProductTypes { get; set; } = new List<ProductType>();
        public TempWeekBasket TempWeekBasket { get; set; }
        public ValidatedWeekBasket ValidatedWeekBasket { get; set; }

        public WeekBasketViewModel()
        {
        }

        public WeekBasketViewModel(AdherentStolon adherentStolon, TempWeekBasket tempWeekBasket, ValidatedWeekBasket validatedWeekBasket, ApplicationDbContext context)
        {
            TempWeekBasket = tempWeekBasket;
            ValidatedWeekBasket = validatedWeekBasket;
	        ActiveAdherentStolon = AdherentStolon = adherentStolon;
            ProductsStocks = context.ProductsStocks
                .Include(x => x.Product)
                .ThenInclude(x => x.Familly)
                .ThenInclude(x => x.Type)
                .Include(x => x.AdherentStolon)
                .Where(x => x.AdherentStolon.StolonId == adherentStolon.Stolon.Id)
                .Where(x => x.State == Product.ProductState.Enabled)
                .AsNoTracking()
                .ToList();

            
            foreach (var familly in ProductsStocks.GroupBy(x=>x.Product.Familly))
            {
                if(!ProductTypes.Any(x=>x.Id == familly.Key.Type.Id))
                {
                    ProductTypes.Add(familly.Key.Type);
                }
             }
        }
    }
}
