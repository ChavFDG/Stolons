using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Adherents
{
    public class AdherentsStolonViewModel : BaseViewModel
    {
        public AdherentsStolonViewModel()
        {

        }
        public AdherentsStolonViewModel(AdherentStolon activeAdherentStolon,Stolon stolon, List<AdherentStolon> adherentsStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentsStolon = adherentsStolon;
            Stolon = stolon;
        }

        public List<AdherentStolon> AdherentsStolon { get; set; }
        public Stolon Stolon { get; set; }
    }
}
