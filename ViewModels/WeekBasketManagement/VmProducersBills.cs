using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasketManagement
{
    public class VmProducersBills : BaseViewModel
    {
        public VmProducersBills(AdherentStolon activeAdherentStolon, List<ProducerBill> bills)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            Bills = bills;
        }

        public List<ProducerBill> Bills { get; set; }
    }
}
