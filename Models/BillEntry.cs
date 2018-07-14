
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Stolons.Models.Product;

namespace Stolons.Models
{
    public class BillEntry : IProduct
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? ConsumerBillId { get; set; }

        [ForeignKey(nameof(ConsumerBillId))]
        public virtual ConsumerBill ConsumerBill { get; set; }

        public Guid? ProducerBillId { get; set; }

        [ForeignKey(nameof(ProducerBillId))]
        public virtual ProducerBill ProducerBill { get; set; }

        public Guid? StolonsBillId { get; set; }

        [ForeignKey(nameof(StolonsBillId))]
        public virtual StolonsBill StolonsBill { get; set; }


        public Guid? TempWeekBasketId { get; set; }

        [ForeignKey(nameof(TempWeekBasketId))]
        public virtual TempWeekBasket TempWeekBasket { get; set; }


        public Guid? ValidatedWeekBasketId { get; set; }

        [ForeignKey(nameof(ValidatedWeekBasketId))]
        public virtual ValidatedWeekBasket ValidatedWeekBasket { get; set; }

        public Guid ProductStockId { get; set; }

        [Display(Name = "Fiche produit")]
        [ForeignKey(nameof(ProductStockId))]
        public ProductStockStolon ProductStock { get; set; }

        [NotMapped]
        public Guid ProductId
        {
            get
            {
                return ProductStock.ProductId;
            }
        }

        [Display(Name = "Quantité")]
        public int Quantity { get; set; }

        [NotMapped]
        public string QuantityString
        {
            get
            {
                return ProductStock.Product.GetQuantityString(Quantity);
            }
        }

        [NotMapped]
        public string QuantityHtmlShortString
        {
            get
            {
                return ProductStock.Product.GetQuantityHtmlShortString(Quantity);
            }
        }

        public decimal WeightPrice { get; set; }

        [NotMapped]
        public Decimal Price
        {
            get
            {
                decimal price = 0;
                if (this.Type == Product.SellType.Piece)
                {
                    price = Quantity * UnitPrice;
                }
                else
                {
                    price = WeightPrice * Quantity * QuantityStep;
                }
                return Quantity * UnitPrice;
            }
        }

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

        [Display(Name = "Stockage")]
        public StorageType Storage { get; set; } = StorageType.Basket;

        [Display(Name = "Type de vente")]
        [Required]
        public SellType Type { get; set; }

        public int ProducerFee { get; set; }
        
        [Display(Name = "Prix unitaire")]
        [Required]
        public decimal UnitPrice { get; set; }
        
        [NotMapped]
        public decimal UnitPriceWithoutTax
        {
            get
            {
                if (Tax == 0)
                    return UnitPrice;
                return UnitPrice- UnitPrice * Tax / 100;
            }
        }
        [NotMapped]
        public decimal PriceWithoutTax
        {
            get
            {
                if (Tax == 0)
                    return Price;
                return Price - Price * Tax / 100;
            }
        }

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

        public string GetFirstImageFullPath()
        {
            if (_Pictures.Any())
            {
                return Path.Combine("uploads", "images", "products", _Pictures[0]+".png");
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

        [Display(Name = "A été modifier")]
        public bool HasBeenModified { get; set; }








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

        public BillEntry Clone()
        {
            BillEntry clonedBillEntry = new BillEntry
            {
                ConsumerBillId = this.ConsumerBillId,
                ProducerBillId = this.ProducerBillId,
                ProductStockId = this.ProductStockId,
                FamillyId = this.FamillyId,
                Familly = this.Familly,
                Name = this.Name,
                WeightPrice = this.WeightPrice,
                UnitPrice = this.UnitPrice,
                Tax = this.Tax,
                TaxEnum = this.TaxEnum,
                ProductUnit = this.ProductUnit,
                Quantity = this.Quantity,
                HasBeenModified = this.HasBeenModified,
                Type = this.Type,
                DLC = this.DLC,
                Storage = this.Storage
            };
            return clonedBillEntry;
        }

        public static BillEntry CloneFromProduct(ProductStockStolon productStock)
        {
            BillEntry billEntry = new BillEntry();
            billEntry.ProductStockId = productStock.Id;
            billEntry.FamillyId = productStock.Product.FamillyId;
            billEntry.Familly = productStock.Product.Familly;
            billEntry.Name = productStock.Product.Name;
            billEntry.WeightPrice = productStock.Product.WeightPrice;
            billEntry.UnitPrice = productStock.Product.UnitPrice;
            billEntry.Tax = productStock.Product.Tax;
            billEntry.TaxEnum = productStock.Product.TaxEnum;
            billEntry.ProductUnit = productStock.Product.ProductUnit;
            billEntry.Type = productStock.Product.Type;
            billEntry.DLC = productStock.Product.DLC;
            billEntry.Storage = productStock.Product.Storage;
            return billEntry;
        }
    }
}
