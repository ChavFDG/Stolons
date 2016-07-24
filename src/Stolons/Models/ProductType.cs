using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class ProductType
    {        
        [Key]
        [Display(Name = "Nom")]
        public string Name { get; set; }
        [Display(Name = "Image")] //Lien vers l'image du label
        public string Image { get; set; }

        //Un type contient des famille de produit
        [Display(Name = "Catégorie du produit")] //Lien vers l'image du label
        public List<ProductFamilly> ProductFamilly { get; set; }

        public ProductType()
        {   
        }

        public ProductType(string name)
        {
            Name = name;
        }
    }
}
