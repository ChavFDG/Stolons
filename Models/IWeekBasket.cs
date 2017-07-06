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
        Adherent Adherent { get; }
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
