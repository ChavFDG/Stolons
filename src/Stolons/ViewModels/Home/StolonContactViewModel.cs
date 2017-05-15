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
        public Dictionary<Adherent,List<Product>> ProducersProd { get;}

        public Dictionary<ProductType, List<ProductFamilly>> AvailableProductFamillyAndType { get; } = new Dictionary<ProductType, List<ProductFamilly>>();

        public StolonContactViewModel(Stolon stolon , Dictionary<Adherent, List<Product>> producersProd, AdherentStolon activeAdherentStolon = null)
        {
            ActiveAdherentStolon = activeAdherentStolon;
            Stolon = stolon;
            ProducersProd = producersProd;
            foreach(var producer in producersProd)
            {
                foreach (var product in producer.Value)
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
