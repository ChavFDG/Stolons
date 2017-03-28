using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Token
{
    public class CreditTokenViewModel
    {
        public CreditTokenViewModel()
        {

        }
        public CreditTokenViewModel(Adherent consumer)
        {
            Consumer = consumer;
        }
        public Adherent Consumer { get; set; }

        [Display(Name ="Bogues à créditer :")]
        public decimal CreditedToken { get; set; } = 0;
    }
}
