using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Stolons.Models;

namespace Stolons.ViewModels.Manage
{
    public class ManageViewModel : BaseViewModel
    {
        public ManageViewModel()
        {

        }
        public ManageViewModel(AdherentStolon activeAdherentStolon, List<AdherentStolon> adherentsStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentsStolon = adherentsStolon;
        }

        public List<AdherentStolon> AdherentsStolon { get; set; }

        public bool IsProducer
        {
            get
            {
                return AdherentsStolon.Any(x => x.IsProducer);
            }
        }
    }
}
