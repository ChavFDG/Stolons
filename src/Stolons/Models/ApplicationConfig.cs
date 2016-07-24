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
        public string StolonsAddress { get; set; } = "Chemin de Saint Clair, 07000 PRIVAS";
        [Display(Name = "Numéro de téléphone de la structure")]
        public string StolonsPhoneNumber { get; set; } = "06 64 86 66 93";

        [Display(Name = "Comission de la structure en % ")]
        public int Comission { get; set; } = 5;

        [Display(Name = "Texte de la page \"qui somme nous\"")]
        public string StolonsAboutPageText { get; set; } = @"Stolons est une association à visé social, étique et solidaire.";
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
        public string OrderDeliveryMessage { get; set; } = "Votre panier est disponible jeudi de 16h à 20 au : chemin de Saint Clair 07000 PRIVAS";

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

    }
}
