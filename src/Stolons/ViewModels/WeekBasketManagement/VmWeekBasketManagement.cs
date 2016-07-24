using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasketManagement
{
    public class VmWeekBasketManagement
    {
        public List<ConsumerBill> ConsumerBills { get; set; }
        public List<ProducerBill> ProducerBills { get; set; }
    }
}
