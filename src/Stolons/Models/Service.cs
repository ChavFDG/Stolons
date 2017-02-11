using Stolons.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Service
    {
        public Service()
        {

        }

        [Key]
        public Guid Id { get; set; }

        public Guid StolonId { get; set; }

        [Display(Name = "Stolon")]
        [ForeignKey(nameof(StolonId))]
        public Stolon Stolon { get; set; }


        [Display(Name = "Nom")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        //Images parties les images présente dans images/services
        public string ImageName { get; set; }
    }
}
