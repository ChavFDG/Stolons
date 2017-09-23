using Stolons.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Adherents
{
    public class SelectAdherentViewModel : BaseViewModel
    {
        public SelectAdherentViewModel()
        {

        }
        public SelectAdherentViewModel(AdherentStolon activeAdherent, Stolon stolon,List<string> adherentsMails, bool addHasProducer)
        {
            base.ActiveAdherentStolon = activeAdherent;
            AdherentsMails = adherentsMails;
            Stolon = stolon;
            AddHasProducer = addHasProducer;
        }

        public List<string> AdherentsMails { get; set; }
        [Display(Name ="Courriel de l'adhérent à ajouter")]
        public string SelectedEmail { get; set; }
        public Stolon Stolon { get; set; }
        public bool AddHasProducer { get; set; }
    }
}
