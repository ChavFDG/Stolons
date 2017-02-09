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

        public int UserKey { get; set; }
        
        [ForeignKey(nameof(UserKey))]
        public StolonsUser User { get; set; }

        public Guid StolonId { get; set; }
        
        [ForeignKey(nameof(StolonId))]
        public Stolon Stolon{ get; set; }
        
        [Required]
        [Display(Name = "Contenue")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Publié le ")]
        public DateTime DateOfPublication { get; set; }
    }
}
