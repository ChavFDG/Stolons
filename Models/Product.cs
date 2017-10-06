using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Product : IProduct
    {

        public  Product()
        {

        }
        [Key]
        public Guid Id { get; set; }

        public List<ProductStockStolon> ProductStocks { get; set; } = new List<ProductStockStolon>();

        [Display(Name = "Producteur")]
        public Guid ProducerId { get; set; }
        [ForeignKey(nameof(ProducerId))]
        public virtual Adherent Producer { get; set; }

        [Display(Name = "Famille de produit")]
        public Guid FamillyId { get; set; }
        [ForeignKey(nameof(FamillyId))]
        public virtual ProductFamilly Familly { get; set; }

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

        [Display(Name = "Stocker dans un réfrégirateur")]
        public bool Fridge { get; set; }

        [Display(Name = "Type de vente")]
        [Required]
        public SellType Type { get; set; }

        [Display(Name = "Masquer le prix au Kilo")]
        [Required]
        public bool HideWeightPrice { get; set; }

        [Display(Name = "Prix (kg ou l)")]
        [Required]
        public decimal WeightPrice { get; set; }

        [Display(Name = "Prix unitaire")]
        [Required]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Gestion du stock")]
        public StockType StockManagement { get; set; } = StockType.Week;

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
                        return QuantityStep + " g";
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
                        return QuantityStep + " ml";
                    }
                }
            }
        }


        [Display(Name = "Quantité moyenne")]
        public int AverageQuantity { get; set; }
        [Display(Name = "Unité de mesure")]
        public Unit ProductUnit { get; set; }

        public decimal Tax { get; set; } = 5.5m;

        [NotMapped]
        [Display(Name = "TVA")]
        public TAX TaxEnum
        {
            get
            {
                if (Tax == 5.5m)
                    return TAX.FiveFive;
                if (Tax == 8.5m)
                    return TAX.EightFive;
                if (Tax == 10m)
                    return TAX.Ten;
                if (Tax == 20m)
                    return TAX.Twenty;
                return TAX.None;
            }
            set
            {
                switch (value)
                {
                    case TAX.None:
                        Tax = 0;
                        break;
                    case TAX.FiveFive:
                        Tax = 5.5m;
                        break;
                    case TAX.EightFive:
                        Tax = 8.5m;
                        break;
                    case TAX.Ten:
                        Tax = 10m;
                        break;
                    case TAX.Twenty:
                        Tax = 20m;
                        break;
                }
            }
        }

        public string GetFirstImage()
        {
            if (_Pictures.Any())
            {
                return _Pictures[0];
            }
            else
            {
                return Configurations.DefaultProductImageFullPath;
            }
        }

        [NotMapped]
        public string LightPath
        {
            get
            {
                return Configurations.ProductsStockagePathLight;
            }
        }

        [NotMapped]
        public string HeavyPath
        {
            get
            {
                return Configurations.ProductsStockagePathHeavy;
            }
        }

        public bool IsModified { get; set; } = false;
        [NotMapped]
        public bool IsAvailable
        {
            get
            {
                return !IsModified;
            }
        }

        public bool IsArchive = false;

        public enum SellType
        {
            [Display(Name = "Au poids")]
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
            MaxHavelaar = 4
        }

        public enum TAX
        {
            [Display(Name = "NA : non assujetti")]
            None = 0,
            [Display(Name = "5,5%")]
            FiveFive = 5,
            [Display(Name = "8,5%")]
            EightFive = 8,
            [Display(Name = "10%")]
            Ten = 10,
            [Display(Name = "20%")]
            Twenty = 20
        }

        public enum StockType
        {
            [Display(Name = "Hebdomadaire", Description = "Le stock est remis à jour chaque semaine au stock initial, il peut évoluer durant la période de commande.")]
            Week = 0,
            [Display(Name = "Fixe", Description = "Le stock est fixe et correspond au stock réel présent dans les locaux de la structure.")]
            Fixed = 1,
            [Display(Name = "Illimité", Description = "Le stock est illimité et n'a pas besoin d'être remis à jour chaque semaine.")]
            Unlimited = 2
        }

        public string GetQuantityString(decimal quantity)
        {
            if (Type == SellType.Piece)
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
                decimal qty = (quantity * QuantityStep);
                string strUnit;
                if (ProductUnit == Product.Unit.Kg)
                {
                    strUnit = " g";
                    if (qty >= 1000)
                    {
                        qty /= 1000;
                        strUnit = " Kg";
                    }
                    return qty + strUnit;
                }
                else
                {
                    strUnit = " mL";
                    if (qty >= 1000)
                    {
                        qty /= 1000;
                        strUnit = " L";
                    }
                    return qty + strUnit;
                }
            }
        }
    }
}
