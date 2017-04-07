using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Stolons
{
    public class StolonViewModel 
    {
        public StolonViewModel()
        {
        }
        public StolonViewModel( Stolon stolon, bool isWebAdmin)
        {
            Stolon = stolon;
            IsWebAdmin = isWebAdmin;
        }
        public Stolon Stolon { get; set; }

        public bool IsWebAdmin { get; set; }
    }
}
