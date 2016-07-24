
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class BillEntry
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }

        [Display(Name = "Fiche produit")]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Display(Name = "Quantité")]
        public int Quantity { get; set; }

	    [NotMapped]
	    public string QuantityString
	    {
	        get
            {
                return Product.GetQuantityString(Quantity);
            }
        }

        [NotMapped]
	    public float Price
	    {
	        get
	        {
		    return (float) Math.Round(Quantity * Product.UnitPrice, 2);
	        }
	    }

        public BillEntry Clone()
        {
            BillEntry clonedBillEntry = new BillEntry();
            clonedBillEntry.ProductId = this.ProductId;
            clonedBillEntry.Product = this.Product;
            clonedBillEntry.Quantity = this.Quantity;
            return clonedBillEntry;
        }
    }
}
