using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Adherents
{
    public class ProducerFeeViewModel : BaseViewModel
    {

        public ProducerFeeViewModel()
        {

        }

        public ProducerFeeViewModel(AdherentStolon activeAdherentStolon ,AdherentStolon adherentStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentStolon = adherentStolon;
            ProducerFee = adherentStolon.ProducerFee;
        }

        public AdherentStolon AdherentStolon { get; set; }


        public int ProducerFee { get; set; }
    }


}
