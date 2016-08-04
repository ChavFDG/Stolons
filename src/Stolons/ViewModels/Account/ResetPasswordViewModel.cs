using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Le mot de passe doit avoir au moins 6 caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
	[Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

	[Required]
        public string Token { get; set; }

	public ResetPasswordViewModel()
	{
	}
	
	public ResetPasswordViewModel(string token, string email)
	{
	    Email = email;
	    Token = token;
	}
    }
}
