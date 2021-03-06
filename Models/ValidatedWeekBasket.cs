﻿using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class ValidatedWeekBasket : IWeekBasket
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AdherentStolonId { get; set; }

        [ForeignKey(nameof(AdherentStolonId))]
        public AdherentStolon AdherentStolon { get; set; }

        // [NotMapped]
        // [Display(Name = "Adherent")]
        // public Adherent Adherent
        // {
        //     get
        //     {
        //         return AdherentStolon.Adherent;
        //     }
        // }

        public List<BillEntry> BillEntries { get; set; }

        [NotMapped]
        public Decimal TotalPrice
        {
            get
            {
                Decimal price = 0;
                foreach (BillEntry entry in BillEntries)
                {
                    price += entry.Price;
                }
                return price;
            }
        }
    }
}
