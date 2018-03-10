using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models;
using Stolons.Models.Users;

namespace Stolons.ViewModels.Banner
{
    public class BannerViewModel : BaseViewModel
    {
        public BannerViewModel(AdherentStolon adherentStolon, TempWeekBasket tempWeekBasket, ConsumerBill consumerBill)
        {
            ActiveAdherentStolon = adherentStolon;
            TempWeekBasket = tempWeekBasket;
            ConsumerBill = consumerBill;

        }

        public TempWeekBasket TempWeekBasket { get; set; }

        public ConsumerBill ConsumerBill { get; set; }
    }
}
