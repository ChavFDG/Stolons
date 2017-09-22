using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Mails
{
    public class MailMessage
    {
        [Display(Name = "Sujet")]
        public string Title { get; set; } = "Entrer le sujet du courriel";
        [Display(Name = "Message")]
        public string Message { get; set; } = "Entrer le message du courriel";
    }
}
