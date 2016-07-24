using Stolons.Models;
using Stolons.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Producers
{
    public class ProducerViewModel : IUserViewModel
    {
        public ProducerViewModel()
        {

        }
        public ProducerViewModel(Producer producer, Configurations.Role userRole)
        {
            Producer = producer;
            OriginalEmail = producer.Email;
            UserRole = userRole;
        }
        

        public string OriginalEmail { get; set; }
        public Producer Producer { get; set; }

        public User User
        {
            get
            {
                return Producer;
            }
        }


        [Display(Name = "Droit utilisateur")]
        public Configurations.Role UserRole { get; set; } = Configurations.Role.User;
    }
}
