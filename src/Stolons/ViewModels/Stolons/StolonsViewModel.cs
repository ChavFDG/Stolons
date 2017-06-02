using Stolons.Models;
using Stolons.Models.Users;
using Stolons.ViewModels.Adherents;
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
        public StolonsViewModel(AdherentStolon activeAdherentStolon, List<AdherentsViewModel> adherentsViewModel)
        {
            AdherentsViewModel = adherentsViewModel;
            ActiveAdherentStolon = activeAdherentStolon;
        }
        public List<AdherentsViewModel> AdherentsViewModel { get; set; }
    }
}
