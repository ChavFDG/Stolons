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
        [Display(Name = "Identifiant")]
        public int LocalId { get; set; }
        //
        public Guid? StolonId { get; set; }
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
            context.SaveChanges();
        }
        //

        [Display(Name = "Est un producteur")]
        public bool IsProducer { get; set; }
        
        [Display(Name = "Commission (en %)")]
        public int ProducerFee { get; set; } = 5;

        [Display(Name = "Role")]
        public Role Role { get; set; } = Role.Adherent;

        //
        [Display(Name = "Activé")]
        public bool Enable { get; set; } = true;

        [Display(Name = "Date d'enregistrement")]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Raison du blocage")]
        public string DisableReason { get; set; }

        public virtual ICollection<News> News { get; set; }

        [Display(Name = "Stols")]
        public decimal Token { get; set; } = 0;

        [Display(Name = "Est supprimer")]
        public bool Deleted { get; set; } = false;


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
            return Configurations.GetSubscriptionAmount(this);
        }
        public string GetStringSubscriptionAmount()
        {
            return Configurations.GetStringSubscriptionAmount(this);
        }

        #endregion Subscription

        //
        public bool Authorized(Role role)
        {
            if (role == Role.Adherent)
                return true;
            else
            {
                if (Adherent.IsWebAdmin)
                    return true;
                return role <= Role;
            }
        }
    }


    public enum Role
    {
        /// <summary>
        /// Simple user 
        /// </summary>
        [Display(Name = "Adhérent")]
        Adherent = 1,
        /// <summary>
        /// Volunteer
        /// </summary>
        [Display(Name = "Bénévole")]
        Volunteer = 2,
        /// <summary>
        /// Admin of the stolon
        /// </summary>
        [Display(Name = "Administrateur")]
        Admin = 3
    }
}
