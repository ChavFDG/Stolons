using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Sympathizers
{
    public class SympathizerViewModel
    {
        public SympathizerViewModel()
        {

        }
        public SympathizerViewModel(Sympathizer sympathizers)
        {
            Sympathizer = sympathizers;
        }
        

        public Sympathizer Sympathizer { get; set; }
    }
}
