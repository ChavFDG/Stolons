using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models.Users;

namespace Stolons.Models
{
    public class Stolon
    {
        [Key]
        public Guid Id { get; set; }

        //Configuration de la structure
        [Display(Name = "Libelle de la structure")]
        public string Label { get; set; } = "Mon stolon";
        [Display(Name = "Adresse de la structure")]
        public string Address { get; set; } = "";
        [Display(Name = "Numéro de téléphone de contact")]
        public string PhoneNumber { get; set; } = "";
        [Display(Name = "Courriel de contact")]
        public string ContactMailAddress { get; set; } = "";
 
        [Display(Name = "Avoir une comission sur les producteurs")]
        public bool UseProducersFee {get;set;} = true;        
        [Display(Name = "Montant de la comission sur les producteurs en %")]
        public int ProducersFee { get; set; } = 5;

        [Display(Name = "Texte de : \"Qui sommes nous ?\"")]
        public string AboutText { get; set; } = @"Texte de présentation de ma structure";

        [Display(Name = "Texte de : \"Rejoingnez nous !\"")]
        public string JoinUsText { get; set; } = @"Texte d'explication sur comment rejoindre la structure";
        //Cotisation

        [Display(Name = "Avoir un système de cotisation")]
        public bool UseSubscipstion { get; set; } = true;

        [Display(Name = "Avoir des sympathisants")]
        public bool UseSympathizer { get; set; } = true;

        [Display(Name = "Cotisation sympathisant (€)")]
        public decimal SympathizerSubscription { get; set; } = 2;
        [Display(Name = "Cotisation consomateur (€)")]
        public decimal ConsumerSubscription { get; set; } = 10;
        [Display(Name = "Cotisation producteur (€)")]
        public decimal ProducerSubscription { get; set; } = 20;
        [Display(Name = "Mois de départ des cotisations")]
        public Month SubscriptionStartMonth { get; set; } = Month.September;
        

        //Site page text
        [Display(Name = "Message de récupération du panier (jour, lieu, plage horraire)")]
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
        public DayOfWeek BasketPickEndUpDay { get; set; } = DayOfWeek.Thursday;
        [Display(Name = "Heure")]
        public int BasketPickUpEndHour { get; set; } = 19;
        [Display(Name = "Minute")]
        public int BasketPickUpEndMinute { get; set; } = 30;

        [Display(Name = "Mode simulation")]
        public bool IsModeSimulated { get; set; } = false;

        [Display(Name = "Choix du mode à simuler")]
        public Modes SimulationMode { get; set; } = Modes.Order;

        [Display(Name = "Mon stolon est en maintenance")]
        public bool IsInMaintenance { get; set; } = false;

        [Display(Name = "Message de maintenance")]
        public string MaintenanceText { get; set; } = "Le site est en maintenance, réouverture prochainement";


        public enum Modes
        {
            [Display(Name = "Commandes")]
            Order = 0,
            [Display(Name = "Livraison et mise à jour des stocks")]
            DeliveryAndStockUpdate = 1
        }
    
        public enum Month
        {
            [Display(Name ="Janvier")]
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
        
        public  Stolon.Modes GetMode()
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

    }
}
