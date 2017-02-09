using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Messages
{
    public class News : Message
    {
        [Required]
        [Display(Name = "Publié comme")]
        public NewsType NewsType { get; set; }

        [Required]
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Display(Name = "Image d'illustration")]
        public string ImageLink { get; set; }

    }

    public enum NewsType
    {
        [Display(Name = "Producteur")]
        Producer = 0 ,
        [Display(Name = "Stolon")]
        Stolon = 1
    }
}

