using Stolons.Models;
using Stolons.Models.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Transactions
{
    public class TransactionViewModel : BaseViewModel
    {
        public TransactionViewModel()
        {

        }

        public TransactionViewModel(AdherentStolon activeAdherentStolon, Stolon stolon, Transaction transaction)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            Stolon = stolon;
            Transaction = transaction;
        }

        public Stolon Stolon{ get; set; }
        public Transaction Transaction { get; set; }
    }
}
