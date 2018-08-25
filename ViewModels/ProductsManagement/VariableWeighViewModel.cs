using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models;
using Stolons.Models.Users;

namespace Stolons.ViewModels.ProductsManagement
{
    public class VariableWeighViewModel : BaseViewModel
    {
        public List<VariableWeighProductViewModel> VariableWeighProductsViewModel { get; set; } = new List<VariableWeighProductViewModel>();

        public VariableWeighViewModel(AdherentStolon activeAdherentStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
        }
    }

    public class VariableWeighProductViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal MinimumWeight { get; set; }
        public decimal MaximumWeight { get; set; }
        public Product.Unit ProductUnit { get; set; }

        public List<ConsumerAssignedWeigh> ConsumersAssignedWeighs { get; set; } = new List<ConsumerAssignedWeigh>();

        public VariableWeighProductViewModel()
        {

        }
    }

    public class ConsumerAssignedWeigh
    {
        public ConsumerAssignedWeigh(BillEntry billEntry)
        {
            BillEntryId = billEntry.Id;
            ConsumerLocalId = billEntry.ConsumerBill.AdherentStolon.LocalId;
        }

        public Guid BillEntryId { get; set; }
        public int ConsumerLocalId { get; set; }
        //En gramme
        public int AssignedWeigh { get; set; }
    }
}
