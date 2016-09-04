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
        [Display(Name = "Numéro de facture")] //Year_WeekNumber_UserId
        string BillNumber { get; set; }
        [Display(Name = "Utilisateur")]
        User User { get; set; }
        [Display(Name = "Etat")]
        BillState State { get; set; }

        [Display(Name = "Date d'édition")]
        DateTime EditionDate { get; set; }
        
        [Display(Name = "Montant")]
        decimal Amount { get; set; }

        string HtmlContent { get; set; }
    }
    public class ConsumerBill : IBill
    {
        [Key]
        [Display(Name = "Numéro de facture")] //Year_WeekNumber_UserId
        public string BillNumber { get; set; }


        [Display(Name = "Consomateur")] //Year_WeekNumber_UserId
        public Consumer Consumer { get; set; }

        [Display(Name = "Date d'édition de la facture")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Etat")]
        public BillState State { get; set; }

        [Display(Name = "Adhérant")]
        [NotMapped]
        public User User
        {
            get
            {
                return Consumer;
            }

            set
            {
                Consumer = value as Consumer;
            }
        }

        [Display(Name = "Montant")]
        public decimal Amount { get; set; }

        public string HtmlContent { get; set; }
    }

    public class ProducerBill : IBill
    {
        [Key]
        [Display(Name = "Numéro de facture")] //Year_WeekNumber_UserId
        public string BillNumber { get; set; }

        [Display(Name = "Producteur")]
        public Producer Producer { get; set; }

        [Display(Name = "Date d'édition de la facture")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Etat")]
        public BillState State { get; set; }

        [NotMapped]
        public User User
        {
            get
            {
                return Producer;
            }

            set
            {
                Producer = value as Producer;
            }
        }

        [Display(Name = "Montant")]
        public decimal Amount { get; set; }

        [Display(Name = "Commission")]
        public int Fee { get; set; }

        [NotMapped]
        public decimal FeeAmount
        {
            get
            {
                return Amount / 100 * Fee;
            }
        }

        [NotMapped]
        public decimal ProducerAmount
        {
            get
            {
                return Amount - (Amount / 100 * Fee);
            }
        }
        public string HtmlContent { get; set; }
    }

    public enum BillState
    {
        [Display(Name = "Edité (attente de livraison / récupération)")]
        Pending = 0,
        [Display(Name = "Livré (attente de payement)")]
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
        public string HtmlContent { get; set; }

        [Key]
        [Display(Name = "Numéro de facture")] //Year_WeekNumber
        public string BillNumber { get; set; }

        [Display(Name = "Date d'édition")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Montant")]
        public decimal Amount { get; set; }

        [Display(Name = "Commission")]
        public int Fee { get; set; }

        [NotMapped]
        public decimal FeeAmount
        {
            get
            {
                return Amount / 100 * Fee;
            }
        }

        [NotMapped]
        public decimal ProducersAmount
        {
            get
            {
                return Amount - (Amount / 100 * Fee);
            }
        }
        
        [NotMapped]
        public string FilePath
        {
            get
            {
                string stolonsBillsPath = Path.Combine(Configurations.Environment.WebRootPath, Configurations.StolonsBillsStockagePath);
                string stolonsBillsFullPath = Path.Combine(stolonsBillsPath, BillNumber + ".pdf");
                return stolonsBillsFullPath;
            }
        }
    }

}
