using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasketManagement
{
    public class VmBillCorrection
    {
        public string Reason { get; set; }

        public List<BillIdQuantity> NewQuantities { get; set; } = new List<BillIdQuantity>();
    }

    public class BillIdQuantity
    {
        public Guid BillId { get; set; }

        public int Quantity { get; set; }
    }
}
