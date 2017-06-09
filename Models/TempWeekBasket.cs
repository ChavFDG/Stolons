using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class TempWeekBasket : IWeekBasket
    {
        [Key]
        public Guid Id { get; set; }

        public AdherentStolon ConsumerStolon { get; set; }

        [Display(Name = "Consomateur")]
        public Adherent Consumer
        {
            get
            {
                return ConsumerStolon.Adherent;
            }
            set
            {
                ConsumerStolon.Adherent = value;
            }
        }

        [Display(Name = "Produits")]
        public List<BillEntry> Products { get; set; }


        public bool Validated { get; set; }
	
	    [NotMapped]
	    public Decimal TotalPrice
	    {
	        get
	        {
                Decimal price = 0;
		        foreach (BillEntry entry in Products)
		        {
		            price += entry.Price;
		        }
		        return price;
	        }
	    }
    }
}
