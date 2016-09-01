using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [NotMapped]
        public IConsumer Consumer
        {
            get
            {
                return User as IConsumer;
            }

            set
            {
                User = value as User;
            }
        }

        [Display(Name = "Date d'édition de la facture")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Etat")]
        public BillState State { get; set; }

        [Display(Name = "Adhérant")]
        public User User { get; set; }

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
}
