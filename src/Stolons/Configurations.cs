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

        #region UserManagement

        public const string Role_User = "User";
        public const string Role_Volunteer = "Volunteer";
        public const string Role_Creator = "Volunteer";
        public const string Role_Administrator = "Administrator";

        public enum Role
        {
            /// <summary>
            /// Simple user 
            /// </summary>
            [Display(Name = "Adhérent")]
            User = 1,
            /// <summary>
            /// Volunteer
            /// </summary>
            [Display(Name = "Bénévole")]
            Volunteer = 2,
            /// <summary>
            /// Creator of the structure
            /// </summary>
            [Display(Name = "Créateur")]
            Creator = 3,
            /// <summary>
            /// Administrator of the web site
            /// </summary>
            [Display(Name = "Administrateur")]
            Administrator = 4
        }

        public const string UserType_SympathizerUser = "Sympathizer";
        public const string UserType_Consumer = "Consumer";
        public const string UserType_Producer  = "Producer";

        public enum UserType
        {
            [Display(Name = "Adhérent Sympathisant")]
            Sympathizer,
            [Display(Name = "Adhérent consomateur")]
            Consumer,
            [Display(Name = "Adhérent producteur")]
            Producer,
        }

        internal static IList<string> GetRoles()
        {
            return Enum.GetNames(typeof(Role));
        }

        internal static IList<string> GetUserTypes()
        {
            return Enum.GetNames(typeof(UserType));
        }
        public static string GetAlias(this StolonsUser user)
        {
            if (user is Producer)
            {
                return (user as Producer).CompanyName;
            }
            else
            {
                return user.Stolon.Label;
            }
        }

        #endregion UserManagement

        #region FileManagement

        public static string StolonsBillsStockagePath = Path.Combine("bills", "stolons");
        public static string ConsumersBillsStockagePath = Path.Combine("bills","consumer");
        public static string ProducersBillsStockagePath = Path.Combine("bills", "producer");
        public static string NewsImageStockagePath = Path.Combine("uploads", "images", "news");
        public static string UserAvatarStockagePath = Path.Combine("uploads", "images", "avatars");
        public static string ProductsTypeAndFamillyIconsStockagesPath = Path.Combine("images", "productFamilies");
        public static string ProductsStockagePathLight = Path.Combine("uploads", "images", "products","light");
        public static string ProductsStockagePathHeavy = Path.Combine("uploads", "images", "products","heavy");
        public static string DefaultProductImage = Path.Combine("uploads", "images", "products", "Default.png");
        public static string DefaultFileName = "Default.png";
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
            string url = GetUrl(bill.User is Consumer ? Configurations.ConsumersBillsStockagePath : Configurations.ProducersBillsStockagePath);
            url += "/" + bill.User.Id + "/" + bill.BillNumber + ".pdf";
            return url;
        }


        public static async Task<string> UploadImageFile(IHostingEnvironment environment, IFormFile uploadFile, string path)
        {
            string uploads = Path.Combine(environment.WebRootPath, path);
            string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');

            string filePath = Path.Combine(uploads, fileName);
            await uploadFile.SaveAsAsync(filePath);

            return Path.Combine(path,fileName);
        }

        public static string UploadImageFile(IHostingEnvironment environment, string base64data ,string path, string fileName)
        {
            base64data = base64data.Remove(0, base64data.IndexOf(',') + 1);
            byte[] data = Convert.FromBase64String(base64data);

            string uploads = Path.Combine(environment.WebRootPath, path);

            string filePath = Path.Combine(uploads, fileName);
            using (var file = File.Create(filePath))
            {
                file.Write(data, 0, data.Count());
            }
            return Path.Combine(path, fileName);
        }

        #endregion FIleManagement

        #region Subscription

        public static decimal GetSubscriptionAmount(StolonsUser user)
        {
            if(user is Sympathizer)
                return user.Stolon.GetSubscriptionAmount(UserType.Sympathizer);
            if (user is Consumer)
                return user.Stolon.GetSubscriptionAmount(UserType.Consumer);
            if (user is Producer)
                return user.Stolon.GetSubscriptionAmount(UserType.Producer);
            return -1;
        }

        public static decimal GetSubscriptionAmount(this Stolon stolon,UserType userType)
        {
            int currentMonth = DateTime.Now.Month -6;
            int subscriptionMonth = (int)stolon.SubscriptionStartMonth;
            if (currentMonth < subscriptionMonth)
                currentMonth += 12;
            bool isHalfSubscription = currentMonth > (subscriptionMonth + 6);

            switch (userType)
            {
                case UserType.Sympathizer:
                    return stolon.SympathizerSubscription;
                case UserType.Consumer:
                    return isHalfSubscription ? stolon.ConsumerSubscription /2 : stolon.ConsumerSubscription;
                case UserType.Producer:
                    return isHalfSubscription ? stolon.ProducerSubscription / 2 : stolon.ProducerSubscription;
            }

            return -1;

        }
        public static string GetStringSubscriptionAmount(StolonsUser user)
        {
            return GetSubscriptionAmount(user) + "€";
        }

        public static string GetStringSubscriptionAmount(this Stolon stolon,UserType userType)
        {
            return stolon.GetSubscriptionAmount(userType) + "€";
        }

        #endregion Subscription

    }
}
