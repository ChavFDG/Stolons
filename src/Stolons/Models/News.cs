using Stolons.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class News
    {
        [Key]
        public Guid Id { get; set; }

        public int UserForeignKey { get; set; }

        [ForeignKey("UserForeignKey")]
        public User User { get; set; }

        [Required]
        [Display(Name = "Titre")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Image d'illustration")]
        public string ImageLink { get; set; }

        [Display(Name = "Publié le ")]
        public DateTime DateOfPublication { get; set; }
    }
}
