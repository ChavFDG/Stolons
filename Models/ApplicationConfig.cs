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

        [Display(Name = "Libellé des Stolons")]
        public string StolonsLabel { get; set; } = "Association Stolons";

        [Display(Name = "Numéro de téléphone de contact")]
        public string ContactPhoneNumber { get; set; } = "06 64 86 66 93";
        [Display(Name = "Courriel de contact")]
        public string ContactMailAddress { get; set; } = "contact@stolons.org";
        //Chromium pour PDF
        [Display(Name = "Chemin complet vers chromium")]
        public string ChromiumFullPath { get; set; } = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
        //Mails
        [Display(Name = "Courriel d'envoi des mails")]
        public string MailAddress { get; set; } = "ne_pas_repondre@stolons.org";
        [Display(Name = "Mot de passe du courriel")]
        public string MailPassword { get; set; } = "Stolons2016";
        [Display(Name = "Smtp")]
        public string MailSmtp { get; set; } = "mail.gandi.net";
        [Display(Name = "Port")]
        public int MailPort { get; set; } = 587;
        [Display(Name = "Site en maintenance")]
        public bool IsInMaintenance{ get; set; } = false;
        [Display(Name = "Message de maintenance du site")]
        public string MaintenanceMessage { get; set; } = "Le site est en maintenance, veuillez nous excuser pour la gêne occasionnée.";
    }
}
