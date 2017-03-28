﻿using Stolons.Models;
using Stolons.Models.Users;
using Stolons.ViewModels.Consumers;
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
        public ProducerViewModel(Adherent producer, Configurations.Role userRole)
        {
            Producer = producer;
            OriginalEmail = producer.Email;
            UserRole = userRole;
        }
        

        public string OriginalEmail { get; set; }
        public Adherent Producer { get; set; }

        public Adherent User
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
