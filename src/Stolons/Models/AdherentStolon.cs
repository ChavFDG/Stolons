using Stolons.Models.Messages;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{

    public class AdherentStolon
    {

        public AdherentStolon()
        {

        }
        public AdherentStolon(Adherent adherent, Stolon stolon, bool isActiveStolon = false)
        {
            Adherent = adherent;
            Stolon = stolon;
            IsActiveStolon = isActiveStolon;
        }


        //Triple clefs
        //
        [Key]
        public Guid Id { get; set; }
        //
        public int LocalId { get; set; }
        //
        public Guid StolonId { get; set; }
        [ForeignKey(nameof(StolonId))]
        public virtual Stolon Stolon { get; set; }
        //
        public Guid AdherentId { get; set; }
        [ForeignKey(nameof(AdherentId))]
        public virtual Adherent Adherent { get; set; }
        
        //
        public bool IsActiveStolon { get; protected set; }
        public void SetHasActiveStolon(ApplicationDbContext context)
        {
            foreach(var adherentStolon in context.AdherentStolons.Where(x=>x.AdherentId == AdherentId))
            {
                adherentStolon.IsActiveStolon = adherentStolon.Id == Id;
            }
        }
        //
        [Display(Name = "Actif / Inactif")]
        public bool Enable { get; set; } = true;

        [Display(Name = "Date d'enregistrement")]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Raison du blocage")]
        public string DisableReason { get; set; }

        public virtual ICollection<News> News { get; set; }

        [Display(Name = "Bogues")]
        public decimal Token { get; set; } = 0;


        #region Consumer

        [Display(Name = "Factures consommateur")]
        public List<ConsumerBill> ConsumerBills { get; set; }

        #endregion Consumer

        #region Producer

        [Display(Name = "Factures producteur")]
        public List<ProducerBill> ProducerBills { get; set; }

        #endregion Producer

        #region Subscription

        [Display(Name = "Cotisation réglée")]
        public bool SubscriptionPaid { get; set; } = false;
        public decimal GetSubscriptionAmount()
        {
            int currentMonth = DateTime.Now.Month - 6;
            int subscriptionMonth = (int)Stolon.SubscriptionStartMonth;
            if (currentMonth < subscriptionMonth)
                currentMonth += 12;
            bool isHalfSubscription = currentMonth > (subscriptionMonth + 6);
            
            if (Adherent is Adherent)
                return isHalfSubscription ? Stolon.ConsumerSubscription / 2 : Stolon.ConsumerSubscription;
            if (Adherent is Adherent)
                return isHalfSubscription ? Stolon.ProducerSubscription / 2 : Stolon.ProducerSubscription;
            return -1;
        }
        public string GetStringSubscriptionAmount(Adherent user)
        {
            return GetSubscriptionAmount() + "€";
        }


        #endregion Subscription

    }
}
