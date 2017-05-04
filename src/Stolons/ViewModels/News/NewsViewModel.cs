using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.News
{
    public class NewsViewModel : BaseViewModel
    {
        public NewsViewModel()
        {

        }

        public NewsViewModel(AdherentStolon activeAdherentStolon, Models.Messages.News news)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            News = news;
        }

        public Models.Messages.News News { get; set; }
    }
}
