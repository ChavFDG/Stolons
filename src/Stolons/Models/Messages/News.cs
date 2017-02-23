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
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Display(Name = "Image d'illustration")]
        public string ImageLink { get; set; }

        [Display(Name = "Date de début de publication")]
        public DateTime PublishStart { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Type de news")]
        public NewsType NewsType { get; set; }

        [Required]
        [Display(Name = "Date de fin de publication")]
        public DateTime PublishEnd { get; set; }

        [Required]
        [Display(Name = "Publié comme")]
        public NewsPublishAs PublishAs { get; set; }


    }

    public enum NewsType
    {
        [Display(Name = "Pour la semaine")]
        Week = 0,
        [Display(Name = "Jusqu'à une date")]
        Highlight = 0
    }
    

    public enum NewsPublishAs
    {
        [Display(Name = "Producteur")]
        Producer = 0 ,
        [Display(Name = "Stolon")]
        Stolon = 1
    }
}

