using Stolons.Models;
using Stolons.ViewModels.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stolons.ViewModels.Chat;

namespace Stolons.ViewModels.Home
{
    public class HomeViewModel : BaseViewModel
    {
        public NewsListViewModel NewsVm { get; set; }
        public ChatMessageListViewModel ChatVm { get;  set; }

        public HomeViewModel()
        {

        }

        public HomeViewModel(AdherentStolon adherentStolon)
        {
            ActiveAdherentStolon = adherentStolon;
        }
    }
}
