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
        public SympathizerViewModel(AdherentStolon activeAdherentStolon, Sympathizer sympathizers)
        {
            Sympathizer = sympathizers;
            ActiveAdherentStolon = activeAdherentStolon;
        }
        

        public Sympathizer Sympathizer { get; set; }
    }
}
