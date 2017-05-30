using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Stolons
{
    public class StolonMembersViewModel 
    {
        public StolonMembersViewModel()
        {
        }
        public StolonMembersViewModel( Stolon stolon,List<AdherentStolon> adherentsStolons, List<Sympathizer> sympathizers, bool isWebAdmin)
        {
            Stolon = stolon;
            IsWebAdmin = isWebAdmin;
            AdherentsStolons = adherentsStolons;
            Sympathizers = sympathizers;
        }
        public Stolon Stolon { get; set; }

        public List<AdherentStolon> AdherentsStolons { get; set; }

        public List<Sympathizer> Sympathizers { get; set; }

        public bool IsWebAdmin { get; set; }
    }
}
