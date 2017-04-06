using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Adherents
{
    public class AdherentViewModel : BaseViewModel
    {
        public AdherentViewModel()
        {

        }
        public AdherentViewModel(AdherentStolon activeAdherentStolon, Adherent adherent)
        {
            Adherent = adherent;
            OriginalEmail = adherent.Email;
            base.ActiveAdherentStolon = activeAdherentStolon;
        }

        public string OriginalEmail { get; set; }

        public Adherent Adherent { get; set; }
    }
}
