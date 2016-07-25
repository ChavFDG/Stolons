using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Helpers
{
    public static class FileHelper
    {
        public static async Task SaveAsAsync(this IFormFile uploadFile, string filePath)
        {
            //TODO a tester
            await uploadFile.CopyToAsync(System.IO.File.Create(filePath)); //Faut il fermer le stream ?

        }
    }
}
