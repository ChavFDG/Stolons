using Stolons.Models;
using Stolons.ViewModels.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Home
{
    public class HomeViewModel : BaseViewModel
    {
        public NewsListViewModel NewsVm { get; set; }
        public HomeViewModel()
        {

        }

        public HomeViewModel(AdherentStolon adherentStolon)
        {
            ActiveAdherentStolon = adherentStolon;
        }
    }
}
