using Stolons.Models;
using Stolons.Models.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Transactions
{
    public class TransactionsViewModel : BaseViewModel
    {
        public TransactionsViewModel()
        {

        }

        public TransactionsViewModel(AdherentStolon activeAdherentStolon, Stolon stolon, List<Transaction> transactions)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            Stolon = stolon;
            Transactions = transactions;
        }

        public Stolon Stolon{ get; set; }
        public List<Transaction> Transactions { get; set; }

        public decimal Balance
        {
            get
            {
                decimal balance = 0;
                Transactions.Where(x=>x.Type == Transaction.TransactionType.Inbound).ToList().ForEach(x => balance += x.Amount);
                Transactions.Where(x => x.Type == Transaction.TransactionType.Outbound).ToList().ForEach(x => balance -= x.Amount);
                return balance;
            }
        }

        public decimal RealBalance
        {
            get
            {
                decimal balance = 0;
                Transactions.Where(x => x.Type == Transaction.TransactionType.Inbound && x.Category != Transaction.TransactionCategory.TokenCredit).ToList().ForEach(x => balance += x.Amount);
                Transactions.Where(x => x.Type == Transaction.TransactionType.Outbound).ToList().ForEach(x => balance -= x.Amount);
                return balance;
            }
        }
    }
}
