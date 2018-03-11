using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models;
using Stolons.Models.Users;

namespace Stolons.ViewModels.Home
{
    public class StolonContactViewModel : BaseViewModel
    {
        public bool IsAdherentLogged
        {
            get
            {
                return this.ActiveAdherentStolon != null;
            }
        }

        public bool IsAdherentMemberOfStolon
        {
            get
            {
                if (!IsAdherentLogged)
                    return false;
                return ActiveAdherentStolon.Adherent.AdherentStolons.Any(x => x.StolonId == Stolon.Id);
            }
        }

        public Stolon Stolon { get; }
        
        public Dictionary<ProductType, List<ProductFamilly>> AvailableProductFamillyAndType { get; } = new Dictionary<ProductType, List<ProductFamilly>>();

        public List<AdherentStolon> Producers { get; set; }

        public int TotalProducts { get; set; }
        public StolonContactViewModel(Stolon stolon , List<AdherentStolon> producers, int totalProducts, AdherentStolon activeAdherentStolon = null)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            Stolon = stolon;
            Producers = producers;
            TotalProducts = totalProducts;

            foreach (var producer in producers)
            {
                foreach (var product in producer.Adherent.Products)
                {
                    if (!AvailableProductFamillyAndType.Keys.Any(x => x.Id == product.Familly.Type.Id))
                        AvailableProductFamillyAndType.Add(product.Familly.Type, new List<ProductFamilly>());

                     if (!AvailableProductFamillyAndType[product.Familly.Type].Any(x=>x.Id == product.Familly.Id))
                        AvailableProductFamillyAndType[product.Familly.Type].Add(product.Familly);
                }
            }
        }

    }
}
