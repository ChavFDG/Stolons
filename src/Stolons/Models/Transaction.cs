using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Type de transaction")]
        public TransactionType Type { get; set; } = TransactionType.Inbound;

        [Required]
        [Display(Name = "Catégorie")]
        public TransactionCategory Category { get; set; } = TransactionCategory.Other;

        [Required]
        [Display(Name = "Montant")]
        public double Amount { get; set; } = 0;

        [Display(Name = "Description")]
        public string Description { get; set; }        

        public enum TransactionType
        {
            [Display(Name = "Entrée")]
            Inbound = 0,
            [Display(Name = "Sortie")]
            Outbound = 1
        }

        public enum TransactionCategory
        {
            [Display(Name = "Cotisation")]
            Subscription = 0,
            [Display(Name = "Loyer")]
            Rent = 1,
            [Display(Name = "Site web")]
            WebSite = 2,
            [Display(Name = "Vente")]
            Sell = 3,
            [Display(Name = "Don")]
            Donation = 4,
            [Display(Name = "Pourcentage")]
            Percentage = 5,
            [Display(Name = "Autre")]
            Other = 99
        }
    }
}
