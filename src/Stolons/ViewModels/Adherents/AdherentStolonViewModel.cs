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

        public AdherentStolonViewModel(AdherentStolon adherentStolon)
        {
            AdherentStolon = adherentStolon;
        }

        public AdherentStolon AdherentStolon { get; set; }
    }


}
