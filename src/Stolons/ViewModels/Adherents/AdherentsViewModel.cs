using Stolons.Models;
using Stolons.Models.Users;
using Stolons.ViewModels.Sympathizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Adherents
{
    public class AdherentsViewModel : BaseViewModel
    {
        public AdherentsViewModel()
        {

        }
        public AdherentsViewModel(AdherentStolon activeAdherentStolon,Stolon stolon, List<Sympathizer> sympathizers, List<AdherentStolon> adherentsStolon)
        {
            AdherentsStolonViewModel = new AdherentsStolonViewModel(activeAdherentStolon,stolon, adherentsStolon);
            ActiveAdherentStolon = activeAdherentStolon;
            SympathizersViewModel = new SympathizersViewModel(activeAdherentStolon,stolon, sympathizers);
            Stolon = stolon;
        }

        public AdherentsStolonViewModel AdherentsStolonViewModel { get; set; }
        public Stolon Stolon { get; set; }

        public SympathizersViewModel SympathizersViewModel { get; set; }

        private List<IAdherent> _iAdherents ;
        public List<IAdherent> IAdherents
        {
            get
            {
                if(_iAdherents == null)
                {
                    _iAdherents = new List<IAdherent>();
                    SympathizersViewModel.Sympathizers.ForEach(x => _iAdherents.Add(x));
                    AdherentsStolonViewModel.AdherentsStolon.ForEach(x => _iAdherents.Add(x.Adherent));
                }
                return _iAdherents;
            }
        }
    }
}
