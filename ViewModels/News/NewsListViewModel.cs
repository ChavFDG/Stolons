using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.News
{
    public class NewsListViewModel : BaseViewModel
    {
        public List<Models.Messages.News> News { get; set; }

        public bool OldNews { get; set; }
        public NewsListViewModel()
        {

        }
        public NewsListViewModel(AdherentStolon activeAdherentStolon, List<Models.Messages.News> news, bool withOldNews = false)
        {
            News = news;
            OldNews = withOldNews;
            ActiveAdherentStolon = activeAdherentStolon;
        }
    }
}
