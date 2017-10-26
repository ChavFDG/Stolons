using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Messages
{
    public class News : Message
    {
        public News()
        {

        }

        public News(Stolon stolon): this()
        {
            var day = stolon.BasketPickEndUpDay - DateTime.Now.DayOfWeek;
            if (day < 0)
                day = 7 + day ;
            var hours =  stolon.BasketPickUpEndHour - DateTime.Now.Hour;
            var minutes =  stolon.BasketPickUpEndMinute - DateTime.Now.Minute;

            var publishEnd = DateTime.Now;
            publishEnd = publishEnd.AddDays(day);
            publishEnd = publishEnd.AddHours(hours);
            publishEnd = publishEnd.AddMinutes(minutes);
            PublishEnd = publishEnd;
        }

        public News(AdherentStolon adherentStolon): this(adherentStolon.Stolon)
        {
            this.PublishBy = adherentStolon;
        }

        [Required]
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Display(Name = "Image d'illustration")]
        public string ImageName { get; set; }

        [NotMapped]
        public string ImageLink
        {
            get
            {
                if (String.IsNullOrWhiteSpace(ImageName))
                    return "\\" + Path.Combine(Configurations.NewsImageStockagePath, Configurations.DefaultImageFileName);
                return "\\" + Path.Combine(Configurations.NewsImageStockagePath, ImageName);
            }
        }

        [Display(Name = "Date de début de publication")]
        public DateTime PublishStart { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Est afficher en surbrillance")]
        public bool IsHighlight { get; set; }

        [Required]
        [Display(Name = "Date de fin de publication")]
        public DateTime PublishEnd { get; set; }

        [Required]
        [Display(Name = "Publié comme")]
        public NewsPublishAs PublishAs { get; set; }

    }


    public enum NewsPublishAs
    {
        [Display(Name = "Producteur")]
        Producer = 0 ,
        [Display(Name = "Stolon")]
        Stolon = 1
    }
}

