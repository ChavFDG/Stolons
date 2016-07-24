using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public interface IWeekBasket
    {
        Guid Id { get; set; }
        Consumer Consumer { get; set; }
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
                    billEntry.Product = context.Products.First(x => x.Id == billEntry.ProductId);
                }
            }
        }
    }

}
