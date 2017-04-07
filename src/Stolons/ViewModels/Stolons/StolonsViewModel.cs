using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Stolons
{
    public class StolonsViewModel : BaseViewModel
    {
        public StolonsViewModel()
        {
        }
        public StolonsViewModel(AdherentStolon activeAdherentStolon, List<Stolon> stolons)
        {
            Stolons = stolons;
            ActiveAdherentStolon = activeAdherentStolon;
        }
        public List<Stolon> Stolons { get; set; }
    }
}
