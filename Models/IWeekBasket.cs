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
        AdherentStolon ConsumerStolon { get; set; }
        Adherent Consumer { get; set; }
        List<BillEntry> Products { get; set; }
    }

    public static class WeekBasketHelper
    {
        public static void RetrieveProducts(this IWeekBasket weekBasket, ApplicationDbContext context)
        {
            foreach (BillEntry billEntry in weekBasket.Products)
            {
                if (billEntry.Product == null)
                {
                    billEntry.Product = context.Products.Include(x=>x.Producer).First(x => x.Id == billEntry.ProductId);
                }
            }
        }
    }

}
