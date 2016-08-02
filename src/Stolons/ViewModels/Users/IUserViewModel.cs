using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Users
{
    public interface IUserViewModel
    {
        SympathizerUser User { get; }
    }
}
