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
        public AdherentsStolonViewModel(AdherentStolon activeAdherentStolon, List<AdherentStolon> adherentsStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentsStolon = adherentsStolon;
        }

        public List<AdherentStolon> AdherentsStolon { get; set; }
    }
}
