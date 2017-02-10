using Stolons.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Service
    {
        public Service()
        {

        }

        [Key]
        public Guid Id { get; set; }

        public Guid StolonId { get; set; }

        [Display(Name = "Stolon")]
        [ForeignKey(nameof(StolonId))]
        public Stolon Stolon { get; set; }


        [Display(Name = "Nom")]
        public string Name
        {
            get
            {
                return EnumHelper<ServiceType>.GetDisplayValue(Type);
            }
        }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Chemin vers l'image")]
        public string ImagePath
        {
            get
            {
                return Path.Combine(Configurations.ServiceImageStockagePath, Type.ToString() +".jpg");
            }
        }

        public ServiceType Type { get; set; }
    }

    public enum ServiceType
    {
        [Display(Name = "Panier")]
        Basket,
        [Display(Name = "Bon plan")]
        GoodPlan,
        [Display(Name = "Bar")]
        Bar,
        [Display(Name = "Concert")]
        Concert,
        [Display(Name = "Conférence")]
        Conference,
        [Display(Name = "Restaurent")]
        Restaurent,
        [Display(Name = "Théatre")]
        Theater
    }
}
