using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Sympathizers
{
    public class SympathizerViewModel : BaseViewModel
    {
        public SympathizerViewModel()
        {

        }
        public SympathizerViewModel(AdherentStolon activeAdherentStolon,Sympathizer sympathizer, Stolon stolon = null)
        {
            Sympathizer = sympathizer;
            ActiveAdherentStolon = activeAdherentStolon;
            Stolon = stolon == null ? Sympathizer.Stolon : stolon;
        }
        

        public Sympathizer Sympathizer { get; set; }
        public Stolon Stolon { get; set; }
    }
}
