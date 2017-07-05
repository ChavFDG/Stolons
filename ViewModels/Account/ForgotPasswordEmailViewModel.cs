using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Stolons.Models.Users;

namespace Stolons.ViewModels.Account
{
    public class ForgotPasswordEmailViewModel
    {

	public string Link { get; set; }

	public Adherent User { get; set; }

	public ForgotPasswordEmailViewModel(Adherent _user, string _link)
	{
	    Link = _link;
	    User = _user;
	}
    }
}
