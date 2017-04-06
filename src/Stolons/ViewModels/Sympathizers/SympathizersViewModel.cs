using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Sympathizers
{
    public class SympathizersViewModel : BaseViewModel
    {
        public SympathizersViewModel()
        {

        }
        public SympathizersViewModel(AdherentStolon activeAdherentStolon, List<Sympathizer> sympathizers)
        {
            Sympathizers = sympathizers;
            ActiveAdherentStolon = activeAdherentStolon;
        }
        

        public List<Sympathizer> Sympathizers { get; set; }
    }
}
