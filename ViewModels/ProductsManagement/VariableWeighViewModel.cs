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
        public List<VariableWeighOrderViewModel> VariableWeighOrdersViewModel { get; set; } = new List<VariableWeighOrderViewModel>();

        public VariableWeighViewModel()
        {

        }
        public VariableWeighViewModel(AdherentStolon activeAdherentStolon)
        {
            ActiveAdherentStolon = activeAdherentStolon;
        }
    }

    public class VariableWeighOrderViewModel
    {
        public Guid ProducerBillId { get; set; }
        public string OrderNumber { get; set; }
        public string StolonLabel { get; set; }
        public List<VariableWeighProductViewModel> VariableWeighProductsViewModel { get; set; } = new List<VariableWeighProductViewModel>();

        public VariableWeighOrderViewModel()
        {

        }
        public VariableWeighOrderViewModel(ProducerBill producerBill)
        {
            ProducerBillId = producerBill.BillId;
            OrderNumber = producerBill.OrderNumber;
            StolonLabel =producerBill.AdherentStolon.Stolon.Label;
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
        public ConsumerAssignedWeigh()
        {

        }
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
