using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class ProductFamilly
    {
        [Key]
        public Guid Id { get; set; }
        [Display(Name = "Nom")]
        public string FamillyName { get; set; }
        [Display(Name = "Type de produit")]
        public ProductType Type { get; set; }

        [Display(Name = "Image")] //Lien vers l'image du label
        public string Image { get; set; } = Path.Combine(Configurations.ProductsTypeAndFamillyIconsStockagesPath, "default.jpg");

        public bool CanBeRemoved { get; set; } = true;

        public ProductFamilly()
        {
        }

        public ProductFamilly(ProductType type, string famillyName)
        {
            Type = type;
            FamillyName = famillyName;
        }

        public ProductFamilly(ProductType type, string famillyName, string image)
        {
            Type = type;
            FamillyName = famillyName;
            Image = image;
        }
    }
}
