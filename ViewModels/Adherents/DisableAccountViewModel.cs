using Stolons.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Adherents
{
    public class DisableAccountViewModel : BaseViewModel
    {
        public DisableAccountViewModel()
        {

        }
        public DisableAccountViewModel(AdherentStolon activeAdherentStolon, AdherentStolon adherentStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentStolon = adherentStolon;
        }
        public AdherentStolon AdherentStolon { get; set; }

        [Display(Name = "Raison :")]
        public string Comment { get; set; } = "Entrer la raison de la désactivation";
    }
}
