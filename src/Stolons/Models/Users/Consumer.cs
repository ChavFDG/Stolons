using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Users
{
    public class Consumer : User,IConsumer
    {
        [Display(Name = "Factures")]
        public List<ConsumerBill> ConsumerBills { get; set; }
    }
}
