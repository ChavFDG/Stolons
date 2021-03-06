﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using static Stolons.Configurations;
using Stolons.Helpers;
using Stolons.Models.Users;
using Stolons.Models;
using Stolons.Services;
using Stolons.Tools;

namespace Stolons
{
    public class Startup
    {
        public IHostingEnvironment _environment;
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            var cultureInfo = new CultureInfo("fr-Fr");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            _environment = env;

            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Storing connection string for reuse in ApplicationDbContext later
            Configurations.DBConnectionString = Configuration.GetConnectionString("Stolons");
            services.AddDbContext<ApplicationDbContext>();

            services.AddMvc().AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            //Password policy
            //stackoverflow.com/questions/27831597/how-do-i-define-the-password-rules-for-identity-in-asp-net-5-mvc-6-vnext
            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    // configure identity options
                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequiredLength = 1;
                    o.Password.RequireNonAlphanumeric = false;
                }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            //services.AddApplicationInsightsTelemetry(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IApplicationLifetime applicationLifetime)
        {
            //Change culture to English
            // Configure the localization options
            app.UseRequestLocalization(new RequestLocalizationOptions() { DefaultRequestCulture = new RequestCulture("fr-FR") });
            //
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Debug);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");

            });
            Configurations.Environment = env;
            Configurations.SetupMailDebug(Configuration.GetSection("MailDebug"));

            applicationLifetime.ApplicationStopping.Register(OnShutdown);
        }

        public static void OnShutdown()
        {
            DotnetHelper.GetLogger<Startup>().LogWarning("Server is shutting down");
            BillGenerator.StopThread();
        }

        public static void SeedDatabase(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();






            List<Stolon> stolons = CreateStolons(dbContext);
            CreateAdminAccount(dbContext, userManager, stolons.First());
            CreateProductCategories(dbContext);
            SetGlobalConfigurations(dbContext);
#if DEBUG
            InitializeSampleAndTestData(dbContext, userManager, stolons.First());
#endif
        }

        #region Stolons config
        private static void InitializeSampleAndTestData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Stolon stolon)
        {
            CreateTestAcount(context, userManager, stolon);
            CreateProductsSamples(context);
        }

        private static void SetGlobalConfigurations(ApplicationDbContext context)
        {

            if (context.ApplicationConfig.Any())
            {
                Configurations.Application = context.ApplicationConfig.First();
            }
            else
            {
                Configurations.Application = new ApplicationConfig();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Configurations.Application.ChromiumFullPath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    Configurations.Application.ChromiumFullPath = @"\usr\bin\chromium\";
                context.Add(Configurations.Application);
                context.SaveChanges();
            }

        }

        private static void CreateProductCategories(ApplicationDbContext context)
        {
            ProductType fresh = CreateProductType(context, "Produits frais");
            CreateProductFamily(context, fresh, "Fruits", "fruits.jpg");
            CreateProductFamily(context, fresh, "Légumes", "legumes.jpg");
            CreateProductFamily(context, fresh, "Produits laitiers", "produits_laitiers.jpg");
            CreateProductFamily(context, fresh, "Oeufs", "oeufs.jpg");
            CreateProductFamily(context, fresh, "Viandes");

            ProductType bakery = CreateProductType(context, "Boulangerie", "boulangerie.jpg");
            CreateProductFamily(context, bakery, "Farines", "farines.jpg");
            CreateProductFamily(context, bakery, "Pains", "pain.jpg");
            CreateProductFamily(context, bakery, "Patisseries");

            ProductType grocery = CreateProductType(context, "Epicerie", "epicerie.jpg");
            CreateProductFamily(context, grocery, "Conserves", "conserves.jpg");
            CreateProductFamily(context, grocery, "Produits secs", "farines.jpg");

            ProductType bevarages = CreateProductType(context, "Boissons", "boissons.jpg");
            CreateProductFamily(context, bevarages, "Alcools", "alcool.jpg");
            CreateProductFamily(context, bevarages, "Sans alcool", "sans_alcool.jpg");

            ProductType other = CreateProductType(context, "Autres", "autres.jpg", false);
            DefaultFamily = CreateProductFamily(context, other, "Non définie", null, false);
            CreateProductFamily(context, other, "Savons", "savon.jpg");

            context.SaveChanges();
        }

        private static ProductType CreateProductType(ApplicationDbContext context, string name, string imageName = null, bool canBeRemoved = true)
        {
            ProductType type = context.ProductTypes.FirstOrDefault(x => x.Name == name);
            if (type == null)
            {
                type = new ProductType(name);
                type.CanBeRemoved = canBeRemoved;
                context.ProductTypes.Add(type);
                if (imageName != null)
                {
                    type.Image = Path.Combine(Configurations.ProductsTypeAndFamillyIconsStockagesPath, imageName);
                }
            }
            return type;
        }

        private static ProductFamilly CreateProductFamily(ApplicationDbContext context, ProductType type, string name, string imageName = null, bool canBeRemoved = true)
        {
            ProductFamilly family = context.ProductFamillys.FirstOrDefault(x => x.FamillyName == name);
            if (family == null)
            {
                family = new ProductFamilly(type, name);
                family.CanBeRemoved = canBeRemoved;
                if (imageName != null)
                {
                    family.Image = Path.Combine(Configurations.ProductsTypeAndFamillyIconsStockagesPath, imageName);
                }
                context.ProductFamillys.Add(family);
            }
            return family;
        }

        private static void CreateProductsSamples(ApplicationDbContext context)
        {
            if (context.Products.Any())
                return;
            Adherent producer = context.AdherentStolons.Include(x => x.Adherent).First(x => x.IsProducer).Adherent;

            Product pain = new Product();
            pain.Name = "Pain complet";
            pain.Description = "Pain farine complete T80";
            pain.Labels.Add(Product.Label.Ab);
            pain.PicturesSerialized = Path.Combine("pain.png");
            pain.WeightPrice = Convert.ToDecimal(15.5);
            pain.UnitPrice = 4;
            pain.TaxEnum = Product.TAX.Ten;
            pain.Producer = producer;
            pain.ProductUnit = Product.Unit.Kg;
            pain.StockManagement = Product.StockType.Unlimited;
            pain.Type = Product.SellType.Piece;
            AddStocks(pain);
            pain.Familly = context.ProductFamillys.First(x => x.FamillyName == "Pains");
            context.Add(pain);
            Product tomate = new Product();
            tomate.Name = "Tomates grappe";
            tomate.Description = "Avec ces tomates, c'est nous qui rougissons même si elles ne sont pas toutes nues!";
            tomate.Labels.Add(Product.Label.Ab);
            tomate.PicturesSerialized = Path.Combine("tomate.jpg");
            tomate.WeightPrice = 3;
            tomate.TaxEnum = Product.TAX.FiveFive;
            tomate.UnitPrice = Convert.ToDecimal(1.5);
            tomate.QuantityStep = 500;
            tomate.Producer = producer;
            tomate.ProductUnit = Product.Unit.Kg;
            tomate.StockManagement = Product.StockType.Week;
            AddStocks(tomate);
            tomate.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            tomate.Type = Product.SellType.Weigh;
            context.Add(tomate);
            Product pommedeterre = new Product();
            pommedeterre.Name = "Pomme de terre";
            pommedeterre.Description = "Pataaaaaaaaaaaaaaaates!!";
            pommedeterre.Labels.Add(Product.Label.Ab);
            pommedeterre.PicturesSerialized = Path.Combine("pommedeterre.jpg");
            pommedeterre.WeightPrice = Convert.ToDecimal(1.99);
            pommedeterre.TaxEnum = Product.TAX.FiveFive;
            pommedeterre.UnitPrice = Convert.ToDecimal(1.99);
            pommedeterre.QuantityStep = 1000;
            pommedeterre.Producer = producer;
            pommedeterre.ProductUnit = Product.Unit.Kg;
            pommedeterre.StockManagement = Product.StockType.Week;
            AddStocks(pommedeterre);
            pommedeterre.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            pommedeterre.Type = Product.SellType.Weigh;
            context.Add(pommedeterre);
            Product radis = new Product();
            radis.Name = "Radis";
            radis.Description = "Des supers radis (pour ceux qui aiment)";
            radis.Labels.Add(Product.Label.Ab);
            radis.PicturesSerialized = Path.Combine("radis.jpg");
            radis.WeightPrice = 0;
            radis.UnitPrice = 4;
            radis.TaxEnum = Product.TAX.FiveFive;
            radis.Producer = producer;
            radis.ProductUnit = Product.Unit.Kg;
            radis.StockManagement = Product.StockType.Week;
            AddStocks(radis);
            radis.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            radis.Type = Product.SellType.Piece;
            context.Add(radis);
            Product salade = new Product();
            salade.Name = "Salade";
            salade.Description = "Une bonne salade pour aller avec les bonnes tomates!";
            salade.Labels.Add(Product.Label.Ab);
            salade.PicturesSerialized = Path.Combine("salade.jpg");
            salade.UnitPrice = Convert.ToDecimal(0.80);
            salade.TaxEnum = Product.TAX.FiveFive;
            salade.WeightPrice = 0;
            salade.Producer = producer;
            salade.ProductUnit = Product.Unit.Kg;
            salade.StockManagement = Product.StockType.Week;
            AddStocks(salade);
            salade.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            salade.Type = Product.SellType.Piece;
            context.Add(salade);
            Product conserveTomate = new Product();
            conserveTomate.Name = "Bocaux de tomate 500ml";
            conserveTomate.Description = "Bocaux de tomate du jardin, cuillie mur et transformé dans la semaine. Bocaux en verre d'une contenance de 500ml";
            conserveTomate.PicturesSerialized = Path.Combine("ConserveTomate.jpg");
            conserveTomate.UnitPrice = Convert.ToDecimal(4);
            conserveTomate.TaxEnum = Product.TAX.None;
            conserveTomate.WeightPrice = 0;
            conserveTomate.Producer = producer;
            conserveTomate.ProductUnit = Product.Unit.L;
            conserveTomate.StockManagement = Product.StockType.Fixed;
            AddStocks(conserveTomate, 30, 0);
            conserveTomate.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            conserveTomate.Type = Product.SellType.Piece;
            context.Add(conserveTomate);


            //
            context.SaveChanges();
        }

        private static void AddStocks(Product product, int remainingStock = 10, int weekStock = 10, Product.ProductState productState = Product.ProductState.Enabled)
        {
            foreach (var adherentStolon in product.Producer.AdherentStolons)
            {
                ProductStockStolon productStock = new ProductStockStolon(product.Id, adherentStolon.Id);
                product.ProductStocks.Add(productStock);
                productStock.RemainingStock = remainingStock;
                productStock.State = productState;
                productStock.WeekStock = weekStock;
            }
        }

        private static List<Stolon> CreateStolons(ApplicationDbContext context)
        {
            if (context.Stolons.Any())
                return context.Stolons.ToList();
            List<Stolon> stolons = new List<Stolon>();
            Stolon stolon = new Stolon();
            stolon.State = Stolon.StolonState.Open;
            stolon.IsModeSimulated = true;
            stolon.CreationDate = DateTime.Now;


            stolon.Label = "Stolon test";
            stolon.AboutText = "Le Stolon de test est une association loi 1901";
            stolon.JoinUsText = "Pour nous rejoindre contacter nous ! Ou venez nous rendre visite";
            stolon.Address = "07000 PRIVAS";
            stolon.PhoneNumber = "06 12 34 56 78";
            stolon.ContactMailAddress = "contact@stolons.org";

            stolon.DeliveryAndStockUpdateDayStartDate = DayOfWeek.Wednesday;
            stolon.DeliveryAndStockUpdateDayStartDateHourStartDate = 12;
            stolon.DeliveryAndStockUpdateDayStartDateMinuteStartDate = 00;
            stolon.OrderDayStartDate = DayOfWeek.Sunday;
            stolon.OrderHourStartDate = 16;
            stolon.OrderMinuteStartDate = 00;

            stolon.BasketPickUpStartDay = DayOfWeek.Thursday;
            stolon.BasketPickUpStartHour = 17;
            stolon.BasketPickUpStartMinute = 30;
            stolon.BasketPickEndUpDay = DayOfWeek.Thursday;
            stolon.BasketPickUpEndHour = 19;
            stolon.BasketPickUpEndMinute = 30;

            stolon.UseSubscipstion = true;
            stolon.SubscriptionStartMonth = Stolon.Month.September;

            stolon.OrderDeliveryMessage = "Votre panier est disponible jeudi de 17h30 à 19h au : adresse, 07000 Privas";

            stolon.UseProducersFee = true;
            stolon.DefaultProducersFee = 5;

            stolon.UseSympathizer = true;
            stolon.SympathizerSubscription = 2;
            stolon.ConsumerSubscription = 10;
            stolon.ProducerSubscription = 20;
            stolon.GoodPlan = true;

            context.Add(stolon);
            context.SaveChanges();

            stolons.Add(stolon);
            return stolons;
        }

        private static void CreateAdminAccount(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Stolon stolon)
        {
            CreateAcount(context,
             userManager,
             "Administrateur",
             "Stolons.org",
             "contact@stolons.org",
             "contact@stolons.org",
             Role.Admin,
             stolon,
             true);
        }

        private static void CreateTestAcount(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Stolon stolon)
        {
            CreateAcount(context,
             userManager,
             "Maurice",
             "Robert",
             "testproducer@yopmail.com",
             "testproducer@yopmail.com",
             Role.Adherent,
             stolon,
             false,
             true,
             true);
        }

        private static void CreateAcount(ApplicationDbContext context, UserManager<ApplicationUser> userManager, string name, string surname, string email, string password, Role role, Stolon stolon, bool isWebAdmin, bool isProducer = false, bool createOnlyIfTableEmpty = false)
        {
            if (createOnlyIfTableEmpty)
            {
                if (isProducer)
                {
                    if (context.AdherentStolons.Any(x => x.IsProducer))
                    {
                        return;
                    }
                }
            }

            if (context.Adherents.Any(x => x.Email == email))
                return;
            Adherent adherent = new Adherent();
            adherent.Name = name;
            adherent.Surname = surname;
            adherent.Email = email;
            adherent.PostCode = "07000";
            adherent.IsWebAdmin = isWebAdmin;

            context.Adherents.Add(adherent);
            context.SaveChanges();

            if (email == "contact@stolons.org")
            {
                bool active = true;
                foreach (Stolon stol in context.Stolons.ToList())
                {
                    AdherentStolon adherentStolon = new AdherentStolon(adherent, stol, active)
                    {
                        RegistrationDate = DateTime.Now,
                        Enable = true,
                        Role = role,
                        LocalId = 0
                    };
                    context.AdherentStolons.Add(adherentStolon);

                    context.SaveChanges();
                    active = false;
                }
            }
            else
            {
                AdherentStolon adherentStolon = new AdherentStolon(adherent, stolon, true)
                {
                    RegistrationDate = DateTime.Now,
                    Enable = true,
                    Role = role
                };
                adherentStolon.LocalId = context.AdherentStolons.Where(x => x.StolonId == stolon.Id).Select(x => x.LocalId).DefaultIfEmpty(0).Max() + 1;

                if (isProducer)
                {
                    adherent.CompanyName = "La ferme de " + adherent.Name;
                    adherent.Latitude = 44.7354673;
                    adherent.Longitude = 4.601407399999971;
                    adherentStolon.IsProducer = true;
                }
                context.AdherentStolons.Add(adherentStolon);
                context.SaveChanges();

            }

            #region Creating linked application data
            var appUser = new ApplicationUser { UserName = adherent.Email, Email = adherent.Email };
            appUser.User = adherent;

            userManager.CreateAsync(appUser, password).Wait();
            #endregion Creating linked application data
        }

        #endregion Stolons config
    }
}
