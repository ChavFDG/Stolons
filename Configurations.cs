using Microsoft.Net.Http.Headers;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Stolons.Helpers;
using Stolons.Models.Users;

namespace Stolons
{
    

    public static class Configurations
    {
        #region Configuration


        public static string SiteUrl
        {
            get
            {
                #if DEBUG
                    return @"localhost:5000";
                #endif
                #if REALEASE
                     return siteWebAddress = @"www.stolons.org";
                #endif
            }
        }

        private static IHostingEnvironment _environment;
        public static IHostingEnvironment Environment
        {
            get
            {
                return _environment;
            }

            set
            {
                _environment = value;
            }
        }
        public static ApplicationConfig Application;

        public static int GetDaysDiff(DayOfWeek from, DayOfWeek to)
        {
            int fromNumber = from == DayOfWeek.Sunday ? 7 : Convert.ToInt32(from);
            int toNumber = to == DayOfWeek.Sunday ? 7 : Convert.ToInt32(to);
            int toReturn = toNumber - fromNumber;
            return toReturn;
        }

        #endregion Configuration

        #region FileManagement

        public static string StolonLogoStockagePath = Path.Combine("uploads", "images","logos");
        public static string ServiceImageStockagePath = Path.Combine("images","services");
        public static string StolonsBillsStockagePath = Path.Combine("bills", "stolons");
        public static string ConsumersBillsStockagePath = Path.Combine("bills","consumer");
        public static string ProducersBillsStockagePath = Path.Combine("bills", "producer");
        public static string NewsImageStockagePath = Path.Combine("uploads", "images", "news");
        public static string AvatarStockagePath = Path.Combine("uploads", "images", "avatars");
        public static string ProductsTypeAndFamillyIconsStockagesPath = Path.Combine("images", "productFamilies");
        public static string ProductsStockagePathLight = Path.Combine("uploads", "images", "products","light");
        public static string ProductsStockagePathHeavy = Path.Combine("uploads", "images", "products","heavy");
        public static string DefaultProductImage = Path.Combine("uploads", "images", "products", "Default.png");
        public static string DefaultFileName = "default.png";
        private static string _labelImagePath = Path.Combine("images", "labels");
        public static string GetImage(this Product.Label label)
        {
            return Path.Combine(_labelImagePath, label.ToString() + ".jpg");
        }

        public static string GetUrl(string localPath)
        {
            return localPath.Replace("\\", "/");
        }

        public static string GetUrl(IBill bill)
        {
            string url = GetUrl(bill is ConsumerBill ? Configurations.ConsumersBillsStockagePath : Configurations.ProducersBillsStockagePath);
            url += "/" + bill.BillNumber + ".pdf";
            return url;
        }


        public static void DeleteFile(this IHostingEnvironment environment, string fullFilePath)
        {
            string toDelete = Path.Combine(environment.WebRootPath, fullFilePath);
            if (File.Exists(toDelete))
                File.Delete(toDelete);
        }

        public static void DeleteFile(this IHostingEnvironment environment,  string filePath,string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                return;
            DeleteFile(environment, Path.Combine(filePath, fileName));
        }

        public static string UploadBase64Image(this IHostingEnvironment environment, string base64data, string path, string pictureName = null)
        {
            base64data = base64data.Remove(0, base64data.IndexOf(',') + 1);
            byte[] data = Convert.FromBase64String(base64data);

            string uploads = Path.Combine(environment.WebRootPath, path);
            if(string.IsNullOrWhiteSpace(pictureName))
                pictureName = Guid.NewGuid().ToString() + ".jpg";
            string filePath = Path.Combine(uploads, pictureName);
            using (var file = File.Create(filePath))
            {
                file.Write(data, 0, data.Count());
            }
            return pictureName;
        }


        public static async Task<string> UploadImageFile(this IHostingEnvironment environment, IFormFile uploadFile, string path)
        {
            string uploads = Path.Combine(environment.WebRootPath, path);
            string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');

            string filePath = Path.Combine(uploads, fileName);
            await uploadFile.SaveAsAsync(filePath);

            return Path.Combine(path,fileName);
        }


        #endregion FileManagement



        public static decimal GetSubscriptionAmount(AdherentStolon adherentStolon)
        {
            int currentMonth = DateTime.Now.Month - 6;
            int subscriptionMonth = (int)adherentStolon.Stolon.SubscriptionStartMonth;
            if (currentMonth < subscriptionMonth)
                currentMonth += 12;
            bool isHalfSubscription = currentMonth > (subscriptionMonth + 6);
            //
            if (adherentStolon.IsProducer)
                return isHalfSubscription ? adherentStolon.Stolon.ProducerSubscription / 2 : adherentStolon.Stolon.ProducerSubscription;
            else
                return isHalfSubscription ? adherentStolon.Stolon.ConsumerSubscription / 2 : adherentStolon.Stolon.ConsumerSubscription;

        }

        public static string GetStringSubscriptionAmount(AdherentStolon adherentStolon)
        {
            return GetSubscriptionAmount(adherentStolon) + "€";
        }

    }
}
