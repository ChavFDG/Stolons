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
        public StolonViewModel(Stolon stolons)
        {
            Stolon = stolons;
        }
        public Stolon Stolon { get; set; }
    }
}
