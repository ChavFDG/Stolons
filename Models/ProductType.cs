using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class ProductType
    {
        [Key]
        public Guid Id { get; set; }
        [Display(Name = "Nom")]
        public string Name { get; set; }
        [Display(Name = "Image")] //Lien vers l'image du label
        public string Image { get; set; } = Path.Combine(Configurations.ProductsTypeAndFamillyIconsStockagesPath, "default.jpg");

        //Un type contient des famille de produit
        [Display(Name = "Catégories du produit")] //Lien vers l'image du label
        public List<ProductFamilly> ProductFamilly { get; set; } = new List<ProductFamilly>();

        public bool CanBeRemoved { get; set; } = true;

        public ProductType()
        {
        }

        public ProductType(string name)
        {
            Name = name;
        }
    }
}
