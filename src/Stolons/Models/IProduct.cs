using System;
using System.Collections.Generic;
using Stolons.Models.Users;

namespace Stolons.Models
{
    public interface IProduct
    {
        int AverageQuantity { get; set; }
        string Description { get; set; }
        DateTime DLC { get; set; }
        ProductFamilly Familly { get; set; }
        Guid FamillyId { get; set; }
        string HeavyPath { get; }
        Guid Id { get; set; }
        bool IsAvailable { get; }
        IList<Product.Label> Labels { get; set; }
        string LabelsSerialized { get; set; }
        string LightPath { get; }
        string Name { get; set; }
        IList<string> Pictures { get; set; }
        string PicturesSerialized { get; set; }
        decimal WeightPrice { get; set; }
        Product.Unit ProductUnit { get; set; }
        int QuantityStep { get; set; }
        string QuantityStepString { get; }
        Product.StockType StockManagement { get; set; }
        decimal Tax { get; set; }
        Product.TAX TaxEnum { get; set; }
        Product.SellType Type { get; set; }
        decimal UnitPrice { get; set; }
        string GetFirstImage();
        string GetQuantityString(int quantity);
    }
}