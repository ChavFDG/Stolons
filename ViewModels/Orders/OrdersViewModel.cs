using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Orders
{
    public class OrdersViewModel : BaseViewModel
    {
        public OrdersViewModel()
        {

        }
        public OrdersViewModel(AdherentStolon activeAdherentStolon,AdherentStolon adherentStolon, List<ConsumerBill> bills)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentStolon = adherentStolon;
            Bills = bills;
        }

        public AdherentStolon AdherentStolon { get; set; }

        public List<ConsumerBill> Bills { get; set; }


    }

}
