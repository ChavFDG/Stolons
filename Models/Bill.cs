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

    public interface IBill
    {
        Guid BillId { get; set; }

        [Display(Name = "Numéro de facture")] //Year_WeekNumber_UserId
        string BillNumber { get; set; }
        [Display(Name = "Utilisateur")]
        AdherentStolon AdherentStolon { get; set; }

        List<BillEntry> BillEntries { get; set; }

        Stolon Stolon { get; }

        [Display(Name = "Etat")]
        BillState State { get; set; }

        [Display(Name = "Date d'édition")]
        DateTime EditionDate { get; set; }

        [Display(Name = "Montant")]
        decimal OrderAmount { get; set; }

        string HtmlBillContent { get; set; }

        [Display(Name ="A été modifié")]
        bool HasBeenModified { get; set; }
        [Display(Name = "Raison de la modification")]
        string ModificationReason { get; set; }
        [Display(Name = "Montant")]
        DateTime ModifiedDate { get; set; }
    }

    public class ConsumerBill : IBill
    {
        [Key]
        public Guid BillId { get; set; }

        [Display(Name = "Numéro de facture")] //Year_WeekNumber_UserId
        public string BillNumber { get; set; }
        public AdherentStolon AdherentStolon { get; set; }
        
        public Adherent Adherent
        {
            get
            {
                return AdherentStolon.Adherent;
            }
        }
        public List<BillEntry> BillEntries { get; set; }

        [Display(Name = "Stolon")]
        public Stolon Stolon
        {
            get
            {
                return AdherentStolon.Stolon;
            }
        }

        [Display(Name = "Date d'édition de la facture")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Etat")]
        public BillState State { get; set; }

        [Display(Name = "Montant")]
        public decimal OrderAmount { get; set; }

        [Display(Name = "Stols")]
        public decimal TokenUsed { get; set; } = 0;

        public string HtmlBillContent { get; set; }

        [Display(Name = "A été modifier")]
        public bool HasBeenModified { get; set; }
        [Display(Name = "Raison de la modification")]
        public string ModificationReason { get; set; }
        [Display(Name = "Date de modification")]
        public DateTime ModifiedDate { get; set; }
    }

    public class ProducerBill : IBill
    {
        [Key]
        public Guid BillId { get; set; }

        [Display(Name = "Numéro de facture")] //Year_WeekNumber_UserId
        public string BillNumber { get; set; }
        public string OrderNumber { get; set; }
        public AdherentStolon AdherentStolon { get; set; }

        public Adherent Adherent
        {
            get
            {
                return AdherentStolon.Adherent;
            }
        }
        
        public List<BillEntry> BillEntries { get; set; }
        
        [Display(Name = "Stolon")]
        public Stolon Stolon
        {
            get
            {
                return AdherentStolon.Stolon;
            }
        }

        [Display(Name = "Date d'édition de la facture")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Etat")]
        public BillState State { get; set; }

        [Display(Name = "Montant de la commande")]
        public decimal OrderAmount { get; set; }

        [Display(Name = "Commission")]
        public int ProducersFee { get; set; }

        [NotMapped]
        public decimal FeeAmount
        {
            get
            {
                return OrderAmount / 100 * ProducersFee;
            }
        }

        [Display(Name = "Montant de la facture (TTC)")]
        [NotMapped]
        public decimal BillAmount
        {
            get
            {
                return Math.Round(OrderAmount - (OrderAmount / 100m * ProducersFee), 2);
            }
        }
        [Display(Name = "Montant de la TVA")]
        public decimal TaxAmount { get; set; }
        public string HtmlBillContent { get; set; }
        public string HtmlOrderContent { get; set; }

        [Display(Name = "A été modifier")]
        public bool HasBeenModified { get; set; }
        [Display(Name = "Raison de la modification")]
        public string ModificationReason { get; set; }
        [Display(Name = "Date de la modification")]
        public DateTime ModifiedDate { get; set; }
    }

    public enum BillState
    {
        [Display(Name = "Edité (attente de livraison / récupération)")]
        Pending = 0,
        [Display(Name = "Livré (attente de paiement)")]
        Delivered = 1,
        [Display(Name = "Payé")]
        Paid = 2
    }

    public class StolonsBill
    {
        public StolonsBill()
        {
            EditionDate = DateTime.Now;
        }

        public StolonsBill(string billNumber) : this()
        {
            this.BillNumber = billNumber;
        }
        public string HtmlBillContent { get; set; }

        [Key]
        [Display(Name = "Numéro de facture")] //Year_WeekNumber
        public string BillNumber { get; set; }

        public Guid StolonId { get; set; }

        [ForeignKey(nameof(StolonId))]
        public Stolon Stolon { get; set; }

        [Display(Name = "Date d'édition")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Montant")]
        public decimal Amount { get; set; }

        [Display(Name = "Commission")]
        public int ProducersFee { get; set; }

        [NotMapped]
        public decimal FeeAmount
        {
            get
            {
                return Amount / 100 * ProducersFee;
            }
        }

        [NotMapped]
        public decimal ProducersAmount
        {
            get
            {
                return Amount - (Amount / 100 * ProducersFee);
            }
        }

        [NotMapped]
        public string FilePath
        {
            get
            {
                return Path.Combine(Configurations.BillsStockagePath, BillNumber + ".pdf");
            }
        }

        [NotMapped]
        public string UrlPath
        {
            get
            {
                return Configurations.GetUrl(FilePath);
            }
        }
    }
}
