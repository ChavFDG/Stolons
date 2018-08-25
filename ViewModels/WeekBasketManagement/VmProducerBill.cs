using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasketManagement
{
    public class VmProducerBill : BaseViewModel
    {
        public VmProducerBill(AdherentStolon activeAdherentStolon, ProducerBill bill)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            Bill = bill;
        }

        public ProducerBill Bill { get; set; }

        public bool Error { get; set; } = false;

        public string ErrorMessage { get; set; } = "Une erreur est survenue";
    }
}
