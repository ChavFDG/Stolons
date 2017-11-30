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
                     return Application.StolonsUrl;
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
        
        public static ProductFamilly DefaultFamily { get; internal set; }
        #endregion Configuration

        #region FileManagement

        public static string StolonLogoStockagePath = Path.Combine("uploads", "images", "logos");
        public static string ServiceImageStockagePath = Path.Combine("images", "services");
        public static string BillsStockagePath = Path.Combine("bills");
        public static string NewsImageStockagePath = Path.Combine("uploads", "images", "news");
        public static string AvatarStockagePath = Path.Combine("uploads", "images", "avatars");
        public static string ProductsTypeAndFamillyIconsStockagesPath = Path.Combine("images", "productFamilies");
        public static string ProductsStockagePathLight = Path.Combine("uploads", "images", "products", "light");
        public static string ProductsStockagePathHeavy = Path.Combine("uploads", "images", "products", "heavy");
        public static string DefaultProductImageFullPath = Path.Combine("uploads", "images", "products", "Default.png");
        public static string DefaultImageFileName = "default.jpg";
        private static string _labelImagePath = Path.Combine("images", "labels");
        public static string GetImage(this Product.Label label)
        {
            return Path.Combine(_labelImagePath, label.ToString() + ".jpg");
        }

        public static string GetUrl(string localPath)
        {
            return localPath.Replace("\\", "/");
        }

        public static string GetBillUrl(IBill bill)
        {
            return BillsStockagePath + "/" + bill.BillNumber + ".pdf";
        }

        public static string GetOrderUrl(ProducerBill prodBill)
        {
            return BillsStockagePath + "/" + prodBill.OrderNumber + ".pdf";
        }

        public static string GetBillFilePath(this IBill bill)
        {
            return Path.Combine(Configurations.Environment.WebRootPath,
                                            Configurations.BillsStockagePath,
                                            bill.GetBillFileName());
        }
        public static string GetOrderFilePath(this ProducerBill bill)
        {
            return Path.Combine(Configurations.Environment.WebRootPath,
                                            Configurations.BillsStockagePath,
                                            bill.GetOrderFileName());
        }

        public static string GetStolonBillFilePath(this StolonsBill bill)
        {
            return Path.Combine(Configurations.Environment.WebRootPath,
                                            Configurations.BillsStockagePath,
                                            bill.GetFileName());
        }

        public static string GetBillFileName(this IBill bill)
        {
            return bill.BillNumber + ".pdf";
        }

        public static string GetOrderFileName(this ProducerBill bill)
        {
            return bill.OrderNumber + ".pdf";
        }

        public static void DeleteFile(this IHostingEnvironment environment, string fullFilePath)
        {
            string toDelete = Path.Combine(environment.WebRootPath, fullFilePath);
            if (File.Exists(toDelete) && !toDelete.Contains(Configurations.DefaultImageFileName))
                File.Delete(toDelete);
        }

        public static void DeleteFile(this IHostingEnvironment environment, string filePath, string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                return;
            DeleteFile(environment, Path.Combine(filePath, fileName));
        }

        public static string UploadBase64Image(this IHostingEnvironment environment, string base64data, string path, string pictureName = null)
        {
            if (string.IsNullOrWhiteSpace(base64data))
                return Configurations.DefaultImageFileName;
            base64data = base64data.Remove(0, base64data.IndexOf(',') + 1);
            byte[] data = Convert.FromBase64String(base64data);

            string uploads = Path.Combine(environment.WebRootPath, path);
            if (string.IsNullOrWhiteSpace(pictureName))
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

            return Path.Combine(path, fileName);
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
