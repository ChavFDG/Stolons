using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.News
{
    public class NewsViewModel
    {
        public List<Models.Messages.News> News { get; set; }

        public bool OldNews { get; set; }
        public NewsViewModel()
        {

        }
        public NewsViewModel(List<Models.Messages.News> news, bool withOldNews = false)
        {
            News = news;
            OldNews = withOldNews;
        }
    }
}
