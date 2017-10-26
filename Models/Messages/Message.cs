using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Messages
{
    public abstract class Message
    {
        [Key]
        public Guid Id { get; set; }        

        public Guid PublishByAdherentStolonId { get; set; }
        
        [ForeignKey(nameof(PublishByAdherentStolonId))]
        public AdherentStolon PublishBy{ get; set; }
        
        [Required]
        [Display(Name = "Contenu")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Publié le ")]
        public DateTime DateOfPublication { get; set; }

        public void Messsage()
        {

        }
    }
}
