using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class ProductFamilly
    {
        [Key]
        [Display(Name = "Nom")]
        public string FamillyName { get; set; }
        [Display(Name = "Type de produit")]
        public ProductType Type { get; set; }
        
        [Display(Name = "Image")] //Lien vers l'image du label
        public string Image { get; set; }

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
