using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.BillHistory
{
    public class BillsViewModel : BaseViewModel
    {
        public BillsViewModel()
        {

        }
        public BillsViewModel(AdherentStolon activeAdherentStolon,AdherentStolon adherentStolon, List<ProducerBill> bills)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            AdherentStolon = adherentStolon;
            Bills = bills;
        }

        public AdherentStolon AdherentStolon { get; set; }

        public List<ProducerBill> Bills { get; set; }


    }

}
