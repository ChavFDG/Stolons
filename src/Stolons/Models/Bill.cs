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
    }
    public class ConsumerBill : IBill
    {
        [Key]
        [Display(Name = "Numéro de facture")] //Year_WeekNumber_UserId
        public string BillNumber { get; set; }

        [Display(Name = "Adhérant")]
        public Consumer Consumer { get; set; }

        [Display(Name = "Date d'édition de la facture")]
        public DateTime EditionDate { get; set; }

        [Display(Name = "Etat")]
        public BillState State { get; set; }

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
