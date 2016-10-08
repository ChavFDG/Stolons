using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class ApplicationConfig
    {

        [Key]
        public Guid Id { get; set; }
        //Configuration de la structure
        [Display(Name = "Libelle de la structure (Stolons)")]
        public string StolonsLabel { get; set; } = "Association Stolons";
        [Display(Name = "Adresse de la structure")]
        public string StolonsAddress { get; set; } = "10 place de l'hotel de ville, 07000 Privas";
        [Display(Name = "Numéro de téléphone de la structure")]
        public string StolonsPhoneNumber { get; set; } = "06 64 86 66 93";

        [Display(Name = "Commission de la structure en % ")]
        public int Fee { get; set; } = 5;

        [Display(Name = "Texte de la page \"qui somme nous\"")]
        public string StolonsAboutPageText { get; set; } = @"Stolons est une association à visé social, étique et solidaire.";
        //Cotisation
        [Display(Name = "Cotisation sympathisant (€)")]
        public decimal SympathizerSubscription { get; set; } = 2;
        [Display(Name = "Cotisation consomateur (€)")]
        public decimal ConsumerSubscription { get; set; } = 10;
        [Display(Name = "Cotisation producteur (€)")]
        public decimal ProducerSubscription { get; set; } = 20;
        [Display(Name = "Mois de départ des cotisations")]
        public Month SubscriptionStartMonth { get; set; } = Month.September;

        //Mails
        [Display(Name = "Courriel de contact")]
        public string ContactMailAddress { get; set; } = "contact@stolons.org";
        [Display(Name = "Courriel d'envoie des mails")]
        public string MailAddress { get; set; } = "ne_pas_repondre@stolons.org";
        [Display(Name = "Mot de passe du courriel")]
        public string MailPassword{ get; set; } = "Stolons2016";
        [Display(Name = "Smtp")] 
        public string MailSmtp { get; set; } = "mail.gandi.net";
        [Display(Name = "Port")]
        public int MailPort{ get; set; } = 587;
        

        //Site page text
        [Display(Name = "Message de récupération du panier (jour, lieu, plage horraire)")]
        public string OrderDeliveryMessage { get; set; } = "Votre panier est disponible jeudi de 17h30 à 19h au : 10 place de l'hotel de ville, 07000 Privas";

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

        [Display(Name = "Mode simulation")]
        public bool IsModeSimulated { get; set; } = false;

        [Display(Name = "Choix du mode à simuler")]
        public Modes SimulationMode { get; set; } = Modes.Order; 

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

    }
}
