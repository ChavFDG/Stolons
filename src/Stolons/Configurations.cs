using Microsoft.Net.Http.Headers;
using Stolons.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Stolons.Helpers;

namespace Stolons
{
    

    public static class Configurations
    {
        #region Configuration
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
        public static ApplicationConfig ApplicationConfig;

        public static int GetDaysDiff(DayOfWeek from, DayOfWeek to)
        {
            int fromNumber = from == DayOfWeek.Sunday ? 7 : Convert.ToInt32(from);
            int toNumber = to == DayOfWeek.Sunday ? 7 : Convert.ToInt32(to);
            int toReturn = toNumber - fromNumber;
            return toReturn;
        }

        public static ApplicationConfig.Modes Mode
        {
            get
            {
                if(ApplicationConfig.IsModeSimulated)
                {
                    return ApplicationConfig.SimulationMode;
                }

                DateTime currentTime = DateTime.Now;

                DateTime deliveryAndStockUpdateStartDate = DateTime.Today;
                deliveryAndStockUpdateStartDate = deliveryAndStockUpdateStartDate.AddDays(GetDaysDiff(currentTime.DayOfWeek, ApplicationConfig.DeliveryAndStockUpdateDayStartDate ));
                deliveryAndStockUpdateStartDate = deliveryAndStockUpdateStartDate.AddHours(ApplicationConfig.DeliveryAndStockUpdateDayStartDateHourStartDate).AddMinutes(ApplicationConfig.DeliveryAndStockUpdateDayStartDateMinuteStartDate);
		
                DateTime orderStartDate = DateTime.Today;
                orderStartDate = orderStartDate.AddDays(GetDaysDiff(currentTime.DayOfWeek, ApplicationConfig.OrderDayStartDate));
                orderStartDate = orderStartDate.AddHours(ApplicationConfig.OrderHourStartDate).AddMinutes(ApplicationConfig.OrderMinuteStartDate);
                
                if (deliveryAndStockUpdateStartDate < orderStartDate)
                {
                    if (deliveryAndStockUpdateStartDate <= currentTime && currentTime <= orderStartDate)
                    {
                        return ApplicationConfig.Modes.DeliveryAndStockUpdate;
                    }
                    else
                    {
                        return ApplicationConfig.Modes.Order;
                    }

                }
                else
                {
                    if (orderStartDate <= currentTime && currentTime <= deliveryAndStockUpdateStartDate)
                    {
                        return ApplicationConfig.Modes.Order;
                    }
                    else
                    {
                        return ApplicationConfig.Modes.DeliveryAndStockUpdate;
                    }
                }
            }
        }



        #endregion Configuration

        #region UserManagement

        public const string Role_User = "User";
        public const string Role_Volunteer = "Volunteer";
        public const string Role_Administrator = "Administrator";

        public enum Role
        {
            [Display(Name = "Adhérent")]
            User = 1,
            [Display(Name = "Bénévole")]
            Volunteer = 2,
            [Display(Name = "Administrateur")]
            Administrator = 3
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
        public static string GetAlias(this Sympathizer user)
        {
            if (user is Producer)
            {
                return (user as Producer).CompanyName;
            }
            else
            {
                return ApplicationConfig.StolonsLabel ;
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
        public static string ProductsStockagePath = Path.Combine("uploads", "images", "products");
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
            url += "/" + bill.User.Id + "/" + bill.BillNumber + ".xlsx";
            return url;
        }

        public static async Task<string> UploadAndResizeImageFile(IHostingEnvironment environment, IFormFile uploadFile, string path)
        {
            string uploads = Path.Combine(environment.WebRootPath, path);
            string fileName = Guid.NewGuid().ToString() + "_" + ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');

            string filePath = Path.Combine(uploads, fileName);
            await uploadFile.SaveAsAsync(filePath);

            return Path.Combine(path,fileName);
        }

        #endregion FIleManagement

    }
}
