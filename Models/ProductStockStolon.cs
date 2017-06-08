using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static Stolons.Models.Product;

namespace Stolons.Models
{
    public class ProductStockStolon
    {

        public ProductStockStolon()
        {

        }
        public ProductStockStolon(Guid productId, Guid adherentStolonId)
        {
            ProductId = productId;
            AdherentStolonId = adherentStolonId;
        }

        [Key]
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        public Guid AdherentStolonId { get; set; }
        [ForeignKey(nameof(AdherentStolonId))]
        public AdherentStolon AdherentStolon { get; set; }


        [Display(Name = "Etat")]
        public ProductState State { get; set; }

        [Display(Name = "Stock de la semaine")]
        public decimal WeekStock { get; set; }
        [Display(Name = "Stock restant")]
        public decimal RemainingStock { get; set; }
    }
}
