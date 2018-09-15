using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Mails
{
    public class MailsSendedVm
    {
        public List<IAdherent> Adherents { get; set; }

        public MailMessage MailMessage { get; set; }

        public MailsSendedVm(List<IAdherent> adherents, MailMessage mailMessage)
        {
            Adherents = adherents;
            MailMessage = mailMessage;
        }
    }
}
