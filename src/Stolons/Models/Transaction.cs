using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Transaction
    {
        public Transaction()
        {

        }
        /// <summary>
        /// Add a new transaction at current date
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <param name="addedAutomaticly"></param>
        public Transaction(TransactionType type, TransactionCategory category, decimal amount, string description, bool addedAutomaticly = true)
        {
            AddedAutomaticly = addedAutomaticly;
            Date = DateTime.Now;
            Type = type;
            Category = category;
            Amount = amount;
            Description = description;
        }

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
        public decimal Amount { get; set; } = 0;

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Ajouter automatiquement")]
        public bool AddedAutomaticly { get; set; } = false;

        public enum TransactionType
        {
            [Display(Name = "Entrée")]
            Inbound = 0,
            [Display(Name = "Sortie")]
            Outbound = 1
        }

        public enum TransactionCategory
        {
            [Display(Name = "Don")]
            Donation = 0,
            [Display(Name = "Loyer")]
            Rent = 1,
            [Display(Name = "Site web")]
            WebSite = 2,
            [Display(Name = "Vente")]
            Sell = 3,
            [Display(Name = "Cotisation")]
            Subscription = 4,
            [Display(Name = "Pourcentage producteur")]
            Fee = 5,
            [Display(Name = "Remboursement producteur")]
            ProducerRefound = 6,
            [Display(Name = "Paiement panier")]
            BillPayement = 7,
            [Display(Name = "Autre")]
            Other = 99
        }
    }
}
