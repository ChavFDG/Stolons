using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Users
{
    public class Sympathizer : IAdherent
    {
        [Key]
        [Display(Name = "Identifiant")]
        public Guid Id { get; set; }

        public Guid StolonId { get; set; }
        [ForeignKey(nameof(StolonId))]
        public virtual Stolon Stolon { get; set; }

        [Display(Name = "Nom")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Prénom")]
        [Required]
        public string Surname { get; set; }

        [Display(Name = "Code postal")]
        [Required]
        public string PostCode { get; set; }

        [Display(Name = "Courriel")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Recevoir les mails d'informations")]
        public bool ReceivedInformationsEmail { get; set; }

        public Configurations.UserType[] UserType
        {
            get
            {
                return new Configurations.UserType[] { Configurations.UserType.Sympathizer };
            }
        }
    }
}
