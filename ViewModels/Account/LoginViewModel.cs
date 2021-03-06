﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Votre courriel")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Votre mot de passe")]
        public string Password { get; set; }

        [Display(Name = "Se souvenir de moi ?")]
        public bool RememberMe { get; set; }

	    public string LoginFailed {get; set;}

        public LoginViewModel()
        {

        }
    }
}
