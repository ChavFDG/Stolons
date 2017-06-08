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
        public SympathizersViewModel(AdherentStolon activeAdherentStolon,Stolon stolon, List<Sympathizer> sympathizers)
        {
            Sympathizers = sympathizers;
            ActiveAdherentStolon = activeAdherentStolon;
            Stolon = stolon;
        }
        

        public List<Sympathizer> Sympathizers { get; set; }
        public Stolon Stolon { get; set; }
    }
}
