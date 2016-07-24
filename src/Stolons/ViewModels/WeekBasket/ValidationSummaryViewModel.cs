using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.WeekBasket
{
    public class ValidationSummaryViewModel
    {
        public ValidatedWeekBasket ValidatedWeekBasket { get; set; }
        public List<BillEntry> UnValidBillEntry { get; set; }

        public float Total { get; set; }

        public bool IsFullValid
        {
            get
            {
                return UnValidBillEntry.Count == 0;
            }
        }
        public ValidationSummaryViewModel()
        {

        }

        public ValidationSummaryViewModel(ValidatedWeekBasket validatedWeekBasket, List<BillEntry> unvalidBillEntry)
        {
            ValidatedWeekBasket = validatedWeekBasket;
            UnValidBillEntry = unvalidBillEntry;
        }
    }
}
