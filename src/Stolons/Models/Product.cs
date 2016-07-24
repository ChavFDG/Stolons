using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }
        [Display(Name = "Producteur")]
        public Producer Producer { get; set; }
        [Display(Name = "Famille de produit")]
        public ProductFamilly Familly { get; set; }
        [Display(Name = "Nom")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }

        private IList<Label> _Labels = new List<Label>();
        [Display(Name = "Labels")]
        [NotMapped]
        public virtual IList<Label> Labels
        {
            get
            {
                return _Labels;
            }
            set
            {
                _Labels = value;
            }
        }

        public string LabelsSerialized
        {
            get
            {
                if (_Labels == null)
                    return null;
                return String.Join(";", _Labels);
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _Labels = new List<Label>();
                }
                else
                {
                    var strings = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    _Labels = new List<Label>();
                    strings.ForEach(x => _Labels.Add((Label)Enum.Parse(typeof(Label), x)));
                }
            }
        }

        private IList<string> _Pictures;
        [Display(Name = "Photos")]
        [NotMapped]
        public virtual IList<string> Pictures
        {
            get
            {
                return _Pictures;
            }
            set
            {
                _Pictures = value;
            }
        }

        public string PicturesSerialized
        {
            get
            {
                return Tools.SerializeListToString(_Pictures);
            }
            set
            {
                _Pictures = Tools.SerializeStringToList(value);
            }
        }

        [Display(Name = "DLC")]
        public DateTime DLC { get; set; }

        [Display(Name = "Type de vente")]
        [Required]
        public SellType Type { get; set; }
        [Display(Name = "Prix (kg ou l)")]
        [Required]
        public float Price { get; set; }
	    [Display(Name = "Prix unitaire")]
	    [Required]
	    public float UnitPrice { get; set; }
        [Display(Name = "Stock de la semaine")]
        public float WeekStock { get; set; }
        [Display(Name = "Stock restant")]
        public float RemainingStock { get; set; }
        [Display(Name = "Palier de poids (g ou ml)")]
	    [Required]
        public int QuantityStep { get; set; }
	    [NotMapped]
	    public string QuantityStepString
	    {
	        get
	        {
		        if (QuantityStep == 0)
		        {
		            return " rien";
		        }
		        if (ProductUnit == Unit.Kg)
		        {
		            if (QuantityStep >= 1000)
		            {
			        return QuantityStep / 1000 + " kg";
		            }
		            else
		            {
			        return QuantityStep  + " g";
		            }
		        }
		        else
		        {
		            if (QuantityStep >= 1000)
		            {
			        return QuantityStep / 1000 + " l";
		            }
		            else
		            {
			        return QuantityStep  + " ml";
		            }
		        }
	        }
	    }


        public string GetQuantityString(int quantity)
        {
            if (Type == Product.SellType.Piece)
            {
                if (quantity == 1)
                {
                    return quantity + " pièce";
                }
                else
                {
                    return quantity + " pièces";
                }
            }
            else
            {
                float qty = (quantity * QuantityStep);
                if (ProductUnit == Product.Unit.Kg)
                {
                    string unit = " g";
                    if (qty >= 1000)
                    {
                        qty /= 1000;
                        unit = " Kg";
                    }
                    return qty + unit;
                }
                else
                {
                    string unit = " mL";
                    if (qty >= 1000)
                    {
                        qty /= 1000;
                        unit = " L";
                    }
                    return qty + unit;
                }
            }
        }

        [Display(Name = "Quantité moyenne")]
        public int AverageQuantity { get; set; }
        [Display(Name = "Unité de mesure")]
        public Unit ProductUnit { get; set; }
        [Display(Name = "Etat")]
        public ProductState State { get; set; }
	

        public string GetFirstImage()
        {
            if(_Pictures.Any())
            {
                return _Pictures[0];
            }
            else
            {
                return Configurations.DefaultProductImage;
            }
        }

        public enum SellType
        {
            [Display(Name ="Au poids")]
            Weigh = 0,
            [Display(Name = "A la pièce")]
            Piece = 1,
            [Display(Name = "Emballé")]
            Wrapped = 2
        }

        public enum Unit
        {
            Kg = 0,
            L = 1
        }

        public enum ProductState
        {
            [Display(Name = "Indisponible")]
            Disabled = 0,
            [Display(Name = "Disponible")]
            Enabled = 1,
            [Display(Name = "Attente Stock")]
            Stock = 2
        }

        public enum Label
        {
            [Display(Name = "AB")]
            Ab = 0,
            [Display(Name = "Demeter")]
            Demeter = 1,
            [Display(Name = "Nature et Progrès")]
            NatureEtProgres = 2,
            [Display(Name = "Fairtrade")]
            Fairtrade = 3,
            [Display(Name = "Max Havelaar ")]
            MaxHavelaar = 4,
            [Display(Name = "Engagé dans le bio")]
            BioEngaged = 5
        }

    }
}
