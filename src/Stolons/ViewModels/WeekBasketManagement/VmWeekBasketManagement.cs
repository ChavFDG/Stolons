using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasketManagement
{
    public class VmWeekBasketManagement
    {
        public Stolon Stolon { get; set; }

        public List<ConsumerBill> ConsumerBills { get; set; }

        public List<ProducerBill> ProducerBills { get; set; }

        public List<StolonsBill> StolonsBills { get; set; }

        public StolonsBill WeekStolonsBill { get; set; }
    }
}
