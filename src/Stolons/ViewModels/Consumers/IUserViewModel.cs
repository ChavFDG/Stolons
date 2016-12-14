using Stolons.Models;
using Stolons.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Consumers
{
    public interface IUserViewModel
    {
        StolonsUser User { get; }
    }
}
