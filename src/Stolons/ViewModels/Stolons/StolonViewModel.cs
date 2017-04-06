using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Stolons
{
    public class StolonViewModel :BaseViewModel
    {
        public StolonViewModel()
        {
        }
        public StolonViewModel(AdherentStolon activeAdherentStolon, Stolon stolons)
        {
            Stolon = stolons;
            ActiveAdherentStolon = activeAdherentStolon;
        }
        public Stolon Stolon { get; set; }
    }
}
