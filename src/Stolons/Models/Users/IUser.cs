using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Users
{
    public interface IUser
    {
        [Key]
        [Display(Name = "Identifiant")]
        int Id { get; set; }

        [Display(Name = "Nom")]
        [Required]
        string Name { get; set; }

        [Display(Name = "Prénom")]
        [Required]
        string Surname { get; set; }

        [Display(Name = "Avatar")]
        string Avatar { get; set; } //Lien vers l'image sur le serveur

        [Display(Name = "Adresse")]
        string Address { get; set; }

        [Display(Name = "Code postal")]
        string PostCode { get; set; }

        [Display(Name = "Ville")]
        string City { get; set; }

        [Display(Name = "Courriel")]
        [EmailAddress]
        string Email { get; set; }

        [Display(Name = "Téléphone")]
        [Phone]
        string PhoneNumber { get; set; }

        [Display(Name = "Cotisation réglée")]
        bool Cotisation { get; set; }

        [Display(Name = "Actif / Inactif")]
        bool Enable { get; set; }

        [Display(Name = "Date d'enregistrement")]
        DateTime RegistrationDate { get; set; }

        [Display(Name = "Raison du blocage")]
        string DisableReason { get; set; }

        List<News> News { get; set; }
    }
}
