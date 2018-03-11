using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.PublicProducers
{
    public class PublicProducersViewModel 
    {
        public PublicProducersViewModel()
        {

        }

        public PublicProducersViewModel(List<AdherentStolon> producers,int totalProducts)
        {
            Producers = producers;
            TotalProducts = totalProducts;
        }

        public List<AdherentStolon> Producers { get; set; }

        public int TotalProducts { get; set; }
    }
}
