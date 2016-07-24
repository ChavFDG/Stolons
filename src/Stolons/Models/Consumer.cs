using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Consumer : User
    {
        [Display(Name = "Factures")]
        public List<ConsumerBill> Bills { get; set; }
    }
}
