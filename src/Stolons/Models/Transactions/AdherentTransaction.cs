using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Transactions
{
    public class AdherentTransaction : Transaction
    {
        public Guid AdherentId { get; set; }
        [ForeignKey(nameof(AdherentId))]
        public Adherent Adherent { get; set; }
    }
}
