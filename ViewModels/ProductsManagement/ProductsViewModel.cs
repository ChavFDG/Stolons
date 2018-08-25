using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models;
using Stolons.Models.Users;

namespace Stolons.ViewModels.ProductsManagement
{
    public class ProductsViewModel : BaseViewModel
    {
        public IEnumerable<Product> Products { get; private set; }
        public Adherent Producer { get; private set; }
        public VariableWeighViewModel VariableWeighViewModel { get; private set; } 

        public ProductsViewModel(AdherentStolon activeAdherentStolon, IEnumerable<Product> products, Adherent producer)
        {
            Products = products;
            Producer = producer;
            ActiveAdherentStolon = activeAdherentStolon;
            VariableWeighViewModel = new VariableWeighViewModel(activeAdherentStolon);
        }
    }
}
