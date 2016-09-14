using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Mails
{
    public class MailMessage
    {
        [Display(Name = "Titre")]
        public string Title { get; set; } = "Entrer le titre du courriel";
        [Display(Name = "Message")]
        public string Message { get; set; } = "Entrer le message du courriel (compatible HTML)";
    }
}
