using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Adherents
{
    public class AdherentStolonViewModel : BaseViewModel
    {

        public AdherentStolonViewModel()
        {

        }

        public AdherentStolonViewModel(AdherentStolon activeAdherentStolon ,AdherentStolon adherentStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentStolon = adherentStolon;
            AdherentViewModel = new AdherentViewModel(activeAdherentStolon, adherentStolon.Adherent, adherentStolon.Stolon, adherentStolon.IsProducer ? AdherentEdition.Producer : AdherentEdition.Consumer,false);
        }

        public AdherentStolon AdherentStolon { get; set; }

        public AdherentViewModel AdherentViewModel { get; set; }
    }


}
