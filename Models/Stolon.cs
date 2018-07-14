using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models.Users;
using System.IO;
using Stolons.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stolons.Models
{
    public class Stolon
    {
        public Stolon()
        {

        }

        [Key]
        public Guid Id { get; set; }

        public List<AdherentStolon> UserStolons { get; set; }
        
        //Configuration de la structure
        [Display(Name = "Libellé de la structure")]
        public string Label { get; set; } = "Mon stolon";
        //Must be unique
        [Display(Name = "Libellé court (utilisé pour la facturation et l'accès)")]
        public string ShortLabel { get; set; } = "MonStol";

        [Display(Name = "Logo de la structure")]
        public string LogoFileName { get; set; }
        public string LogoFilePath
        {
            get
            {
                if (String.IsNullOrWhiteSpace(LogoFileName))
                    return "\\" + Path.Combine(Configurations.StolonLogoStockagePath, Configurations.DefaultImageFileName);
                return "\\" + Path.Combine(Configurations.StolonLogoStockagePath, LogoFileName);
            }
        }
        [Display(Name = "Adresse de la structure")]
        public string Address { get; set; } = "";
        [Display(Name = "Téléphone de contact")]
        public string PhoneNumber { get; set; } = "";
        [Display(Name = "Courriel de contact")]
        public string ContactMailAddress { get; set; } = "";
        [Display(Name = "Page Facebook")]
        public string FacebookPage { get; set; } = "";

        [Display(Name = "Avoir une commission sur les producteurs")]
        public bool UseProducersFee { get; set; } = true;
        [Display(Name = "Montant de la commission par défault sur les producteurs (en %)")]
        public int DefaultProducersFee { get; set; } = 5;

        [Display(Name = "Texte de : \"Qui sommes nous ?\"")]
        public string AboutText { get; set; } = @"Texte de présentation de ma structure";

        [Display(Name = "Texte de : \"Rejoignez nous !\"")]
        public string JoinUsText { get; set; } = @"Texte d'explication sur comment rejoindre la structure";

        [Display(Name = "Avoir des sympathisants")]
        public bool UseSympathizer { get; set; } = true;

        //Cotisation

        [Display(Name = "Avoir un système de cotisation")]
        public bool UseSubscipstion { get; set; } = true;

        [Display(Name = "Cotisation sympathisant (€)")]
        public decimal SympathizerSubscription { get; set; } = 2;
        [Display(Name = "Cotisation consommateur (€)")]
        public decimal ConsumerSubscription { get; set; } = 10;
        [Display(Name = "Cotisation producteur (€)")]
        public decimal ProducerSubscription { get; set; } = 20;
        [Display(Name = "Mois de départ des cotisations")]
        public Month SubscriptionStartMonth { get; set; } = Month.September;
        [Display(Name = "Réduire la cotisation de moitié à mis année")]
        public bool UseHalftSubscipstion { get; set; } = false;


        //Site page text
        [Display(Name = "Message de récupération du panier (jour, lieu, plage horaire)")]
        public string OrderDeliveryMessage { get; set; } = "Votre panier est disponible :...";

        //ORDER
        [Display(Name = "Jour")]
        public DayOfWeek OrderDayStartDate { get; set; } = DayOfWeek.Sunday;
        [Display(Name = "Heure")]
        public int OrderHourStartDate { get; set; } = 16;
        [Display(Name = "Minute")]
        public int OrderMinuteStartDate { get; set; } = 0;

        //DeliveryAndStockUpdate
        [Display(Name = "Jour")]
        public DayOfWeek DeliveryAndStockUpdateDayStartDate { get; set; } = DayOfWeek.Wednesday;
        [Display(Name = "Heure")]
        public int DeliveryAndStockUpdateDayStartDateHourStartDate { get; set; } = 12;
        [Display(Name = "Minute")]
        public int DeliveryAndStockUpdateDayStartDateMinuteStartDate { get; set; } = 0;

        //Basket pickup time
        [Display(Name = "Jour")]
        public DayOfWeek BasketPickUpStartDay { get; set; } = DayOfWeek.Thursday;
        [Display(Name = "Heure")]
        public int BasketPickUpStartHour { get; set; } = 17;
        [Display(Name = "Minute")]
        public int BasketPickUpStartMinute { get; set; } = 30;
	    [Display(Name = "Jour")]
        public DayOfWeek BasketPickEndUpDay { get; set; } = DayOfWeek.Thursday;
        [Display(Name = "Heure")]
        public int BasketPickUpEndHour { get; set; } = 19;
        [Display(Name = "Minute")]
        public int BasketPickUpEndMinute { get; set; } = 30;

        [Display(Name = "Mode simulation")]
        public bool IsModeSimulated { get; set; } = false;

        [Display(Name = "Choix du mode à simuler")]
        public Modes SimulationMode { get; set; } = Modes.Order;

        [Display(Name = "Etat du stolon")]
        public StolonState State { get; set; } = StolonState.Closed;

        [Display(Name = "Message d'état du stolon")]
        public string StolonStateMessage { get; set; } = "Le site est en maintenance, réouverture prochainement";

        [Display(Name = "Latitude")]
        public double Latitude { get; set; }
        [Display(Name = "Longitude")]
        public double Longitude { get; set; }

        [Display(Name = "Services proposés")]
        public List<Service> Services { get; set; } = new List<Service>();

        [Display(Name = "Bons plan")]
        public bool GoodPlan { get; set; }

        [Display(Name = "Type de stolon")]
        public OrganisationType StolonType { get; set; } = OrganisationType.Association;

        [Display(Name = "Cotisation payée")]
        public bool SubscriptionPaid { get; set; } = false;

        [Display(Name = "Date de création")]
        public DateTime CreationDate { get; set; }

        public enum OrganisationType
        {
            [Display(Name = "Association")]
            Association = 0,
            [Display(Name = "Producteur")]
            Producer = 1
        }

        public enum StolonState
        {
            [Display(Name = "Fermé")]
            Closed = 0,
            [Display(Name = "Ouvert")]
            Open = 1
        }

        public enum Modes
        {
            [Display(Name = "Commandes")]
            Order = 0,
            [Display(Name = "Livraison et mise à jour des stocks")]
            DeliveryAndStockUpdate = 1
        }

        public enum Month
        {
            [Display(Name = "Janvier")]
            January = 1,
            [Display(Name = "Février")]
            February = 2,
            [Display(Name = "Mars")]
            March = 3,
            [Display(Name = "Avril")]
            April = 4,
            [Display(Name = "Mai")]
            May = 5,
            [Display(Name = "Juin")]
            June = 6,
            [Display(Name = "Juillet")]
            July = 7,
            [Display(Name = "Aout")]
            August = 8,
            [Display(Name = "Septembre")]
            September = 9,
            [Display(Name = "Octobre")]
            October = 10,
            [Display(Name = "Novembre")]
            November = 11,
            [Display(Name = "Decembre")]
            December = 12
        }

        public string GetStringPickUpTime()
        {
            string toReturn = BasketPickUpStartDay.ToFrench() + " de " + String.Format("{0:00}", BasketPickUpStartHour) + "h" + String.Format("{0:00}", BasketPickUpStartMinute);

            if (BasketPickEndUpDay != BasketPickUpStartDay)
            {
                toReturn += " au " + BasketPickEndUpDay.ToFrench();
            }
            toReturn += " à " + String.Format("{0:00}", BasketPickUpEndHour) + "h" + String.Format("{0:00}", BasketPickUpEndMinute);
            return toReturn;
        }

	[NotMapped]
	public String StringPickupTime {get;set;}

        public string GetStringOrderTime()
        {
            string toReturn = OrderDayStartDate.ToFrench() + " de " + String.Format("{0:00}", OrderHourStartDate) + "h" + String.Format("{0:00}", OrderMinuteStartDate);
            toReturn += " au " + DeliveryAndStockUpdateDayStartDate.ToFrench() + " à " + String.Format("{0:00}", DeliveryAndStockUpdateDayStartDateHourStartDate) + "h" + String.Format("{0:00}", DeliveryAndStockUpdateDayStartDateMinuteStartDate);
            return toReturn;
        }

        [NotMapped]
        public Modes Mode
        {
            get
            {
                return GetMode();
            }
        }

        public Modes GetMode()
        {
            if (IsModeSimulated)
            {
                return SimulationMode;
            }

            DateTime currentTime = DateTime.Now;

            DateTime deliveryAndStockUpdateStartDate = DateTime.Today;
            deliveryAndStockUpdateStartDate = deliveryAndStockUpdateStartDate.AddDays(Configurations.GetDaysDiff(currentTime.DayOfWeek, DeliveryAndStockUpdateDayStartDate));
            deliveryAndStockUpdateStartDate = deliveryAndStockUpdateStartDate.AddHours(DeliveryAndStockUpdateDayStartDateHourStartDate).AddMinutes(DeliveryAndStockUpdateDayStartDateMinuteStartDate);

            DateTime orderStartDate = DateTime.Today;
            orderStartDate = orderStartDate.AddDays(Configurations.GetDaysDiff(currentTime.DayOfWeek, OrderDayStartDate));
            orderStartDate = orderStartDate.AddHours(OrderHourStartDate).AddMinutes(OrderMinuteStartDate);

            if (deliveryAndStockUpdateStartDate < orderStartDate)
            {
                if (deliveryAndStockUpdateStartDate <= currentTime && currentTime <= orderStartDate)
                {
                    return Modes.DeliveryAndStockUpdate;
                }
                else
                {
                    return Modes.Order;
                }

            }
            else
            {
                if (orderStartDate <= currentTime && currentTime <= deliveryAndStockUpdateStartDate)
                {
                    return Modes.Order;
                }
                else
                {
                    return Modes.DeliveryAndStockUpdate;
                }
            }
        }

	[NotMapped]
	public List<AdherentStolon> Producers {get; set;}

	[NotMapped]
	public List<ProductStockStolon> Products {get; set;}
    }
}
