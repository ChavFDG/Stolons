using MoreLinq;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasketManagement
{
    public class VmWeekBasketHistory : BaseViewModel
    {
        public VmWeekBasketHistory(AdherentStolon activeAdherentStolon, List<StolonsBill> stolonsBills, List<ConsumerBill> consumerBills , List<ProducerBill> producerBills)
        {
            ActiveAdherentStolon = activeAdherentStolon;

            foreach(var stolonsBill in stolonsBills)
            {
                ProdConsumBillsValues prodConsumBillsValues = new ProdConsumBillsValues();
                //Consumer bills
                var consBillId = stolonsBill.BillEntries.DistinctBy(x => x.ConsumerBillId).Select(x=>x.ConsumerBillId);
                List<ConsumerBill> consBills = new List<ConsumerBill>();
                consumerBills.Where(x => consBillId.Contains(x.BillId)).ForEach(bill => consBills.Add(bill));
                //Producer bills
                var prodBillId = stolonsBill.BillEntries.DistinctBy(x => x.ProducerBillId).Select(x => x.ProducerBillId);
                List<ProducerBill> prodBills = new List<ProducerBill>();
                producerBills.Where(x => prodBillId.Contains(x.BillId)).ForEach(bill => prodBills.Add(bill));
                //Re arange
                HistoryBills.Add(stolonsBill, new ProdConsumBillsValues() { ConsumerBills = consBills, ProducerBills = prodBills });
            }

            ProducerBills = producerBills;
        }

        public Stolon Stolon { get; set; }

        public Dictionary<StolonsBill, ProdConsumBillsValues> HistoryBills { get; set; } = new Dictionary<StolonsBill, ProdConsumBillsValues>();

        public List<ProducerBill> ProducerBills { get; set; }
    }

    public class ProdConsumBillsValues
    {
        public List<ConsumerBill> ConsumerBills { get; set; }

        public List<ProducerBill> ProducerBills { get; set; }
    }
}
