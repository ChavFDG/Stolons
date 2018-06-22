using Microsoft.EntityFrameworkCore;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public interface IWeekBasket
    {
        Guid Id { get; set; }
        Guid AdherentStolonId { get; set; }
        AdherentStolon AdherentStolon { get; set; }
        //Adherent Adherent { get; }
        List<BillEntry> BillEntries { get; set; }
    }

    public static class WeekBasketHelper
    {
        public static void RetrieveProducts(this IWeekBasket weekBasket, ApplicationDbContext context)
        {
            foreach (BillEntry billEntry in weekBasket.BillEntries)
            {
                if (billEntry.ProductStock == null)
                {
                    billEntry.ProductStock = context.ProductsStocks.Include(x => x.AdherentStolon).Include(x => x.Product).Include(x => x.AdherentStolon.Adherent).AsNoTracking().First(x => x.Id == billEntry.ProductStockId);
                }
            }
        }
        
    }

}
