using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Manage
{
    public class ChangeEmailViewModel
    {
        public Guid UserId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Nouveau courriel")]
        public string NewEmail { get; set; }
    }
}
