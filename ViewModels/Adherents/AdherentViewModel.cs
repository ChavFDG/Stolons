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
        public AdherentViewModel(AdherentStolon activeAdherentStolon,Adherent adherent, Stolon stolon, AdherentEdition edition)
        {
            Adherent = adherent;
            OriginalEmail = adherent.Email;
            ActiveAdherentStolon = activeAdherentStolon;
            Edition = edition;
            Stolon = stolon;
        }

        public string OriginalEmail { get; set; }

        public Adherent Adherent { get; set; }
        public AdherentEdition Edition { get; set; }
        public Stolon Stolon { get; set; }

    }
    public enum AdherentEdition
    {
        Consumer,
        Producer
    }
}
