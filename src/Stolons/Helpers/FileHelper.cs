using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Helpers
{
    public static class FileHelper
    {
        public static async Task SaveAsAsync(this IFormFile uploadFile, string filePath)
        {
            using (var stream = System.IO.File.Create(filePath))
            {
                await uploadFile.CopyToAsync(stream); 
            }

        }
        public static async Task SaveImageAsAsync(this IFormFile uploadFile, string filePath)
        {
            //Todo utiliser ImageProcessorCore quand il sera dispo...
            //https://github.com/JimBobSquarePants/ImageProcessor
            //Voir les méthodes utilisants : UploadAndResizeImageFile dans Configurations
            await SaveAsAsync(uploadFile, filePath);

        }
    }
}
