using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models;

namespace Stolons.ViewModels.Banner
{
    public class BannerViewModel
    {
        public SympathizerUser User { get; set; }

        public BannerViewModel(SympathizerUser user)
        {
            User = user;
        }
    }
}
