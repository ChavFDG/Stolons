using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models;
using Stolons.Models.Users;

namespace Stolons.ViewModels.Banner
{
    public class BannerViewModel
    {
        public AdherentStolon AdherentStolon{ get; set; }

        public BannerViewModel(AdherentStolon adherentStolon)
        {
            AdherentStolon = adherentStolon;
        }
    }
}
