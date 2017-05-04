using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Users
{
    public interface IAdherent
    {
        [Key]
        Guid Id { get; set; }

        [Display(Name = "Nom")]
        [Required]
        string Name { get; set; }

        [Display(Name = "Prénom")]
        [Required]
        string Surname { get; set; }

        [Display(Name = "Code postal")]
        [Required]
        string PostCode { get; set; }

        [Display(Name = "Courriel")]
        [EmailAddress]
        string Email { get; set; }

        [Display(Name = "Recevoir les mails d'informations")]
        bool ReceivedInformationsEmail { get; set; }
        
    }
}
