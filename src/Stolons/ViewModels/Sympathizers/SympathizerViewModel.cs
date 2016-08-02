using Stolons.Models;
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
            Sympathizers = sympathizers;
            OriginalEmail = sympathizers.Email;
        }

        public string OriginalEmail { get; set; }

        public Sympathizer Sympathizers { get; set; }

        public Sympathizer Sympathizer
        {
            get
            {
                return Sympathizers;
            }
        }
    }
}
