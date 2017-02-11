using System;
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
using Stolons.Models;
using Stolons.Services;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using System.Threading;
using Stolons.Tools;
using Stolons.Models.Users;
using Microsoft.Extensions.Configuration.UserSecrets;

[assembly: UserSecretsId("aspnet-Stolons-ce345b64-19cf-4972-b34f-d16f2e7976ed")]

namespace Stolons
{
    public class Startup
    {

        IHostingEnvironment _environment;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                //builder.AddUserSecrets(); // http://stackoverflow.com/questions/40858155/ef-core-1-1-to-webapi-core-add-migration-fails
                builder.AddUserSecrets<Startup>();
            }
            _environment = env;
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {

                string test = Configuration["Data:DefaultConnection:SqLiteConnectionString"];

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite("Data Source=" + Path.Combine(_environment.ContentRootPath, "Stolons.sqlite")));

                /*
                // Add framework services.
                if (Configuration["Data:UseSqLite"] == "true")
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite("Data Source=" + Path.Combine(_environment.ContentRootPath, "Stolons.sqlite")));
                    //options.UseSqlite(Configuration["Data:DefaultConnection:SqLiteConnectionString"]));
                }
                else
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration["Data:DefaultConnection:MsSqlConnectionString"]));
                }*/

                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                services.AddMvc();

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
                }).AddDefaultTokenProviders();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            //Change culture to English
            // Configure the localization options
            app.UseRequestLocalization(new RequestLocalizationOptions() { DefaultRequestCulture = new RequestCulture("En-Gb") });
            //
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                    }
                }
                catch { }
            }
            //app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            await CreateRoles(serviceProvider);
            List<Stolon> stolons = CreateStolons(context);
            await CreateAdminAccount(context, userManager, stolons.First());
            CreateProductCategories(context);
            SetGlobalConfigurations(context);
#if DEBUG
            await InitializeSampleAndTestData(serviceProvider, context, userManager, stolons.First());
#endif
            Thread billManager = new Thread(() => BillGenerator.ManageBills(context));
            Configurations.Environment = env;
            billManager.Start();
        }


        #region Stolons config
        private async Task InitializeSampleAndTestData(IServiceProvider serviceProvider, ApplicationDbContext context, UserManager<ApplicationUser> userManager, Stolon stolon)
        {
            await CreateTestAcount(context, userManager, stolon);
            CreateProductsSamples(context);
        }

        private void SetGlobalConfigurations(ApplicationDbContext context)
        {

            if (context.ApplicationConfig.Any())
            {
                Configurations.Application = context.ApplicationConfig.First();
            }
            else
            {
                Configurations.Application = new ApplicationConfig();
                context.Add(Configurations.Application);
                context.SaveChanges();
            }

        }

        private void CreateProductCategories(ApplicationDbContext context)
        {
            ProductType fresh = CreateProductType(context, "Produits frais");
            CreateProductFamily(context, fresh, "Fruits", "fruits.jpg");
            CreateProductFamily(context, fresh, "Légumes", "legumes.jpg");
            CreateProductFamily(context, fresh, "Produits laitiers", "produits_laitiers.jpg");
            CreateProductFamily(context, fresh, "Oeufs", "oeufs.jpg");

            ProductType bakery = CreateProductType(context, "Boulangerie", "boulangerie.jpg");
            CreateProductFamily(context, bakery, "Farines", "farines.jpg");
            CreateProductFamily(context, bakery, "Pains", "pain.jpg");

            ProductType grocery = CreateProductType(context, "Epicerie", "epicerie.jpg");
            CreateProductFamily(context, grocery, "Conserves", "conserves.jpg");

            ProductType bevarages = CreateProductType(context, "Boissons", "boissons.jpg");
            CreateProductFamily(context, bevarages, "Alcools", "alcool.jpg");
            CreateProductFamily(context, bevarages, "Sans alcool", "sans_alcool.jpg");

            ProductType other = CreateProductType(context, "Autres", "autres.jpg");
            CreateProductFamily(context, other, "Savons", "savon.jpg");

            context.SaveChanges();
        }

        private ProductType CreateProductType(ApplicationDbContext context, string name, string imageName = null)
        {
            ProductType type = context.ProductTypes.FirstOrDefault(x => x.Name == name);
            if (type == null)
            {
                type = new ProductType(name);
                context.ProductTypes.Add(type);
                if (imageName != null)
                {
                    type.Image = Path.Combine(Configurations.ProductsTypeAndFamillyIconsStockagesPath, imageName);
                }
            }
            return type;
        }

        private ProductFamilly CreateProductFamily(ApplicationDbContext context, ProductType type, string name, string imageName = null)
        {
            ProductFamilly family = context.ProductFamillys.FirstOrDefault(x => x.FamillyName == name);
            if (family == null)
            {
                family = new ProductFamilly(type, name);
                if (imageName != null)
                {
                    family.Image = Path.Combine(Configurations.ProductsTypeAndFamillyIconsStockagesPath, imageName);
                }
                context.ProductFamillys.Add(family);
            }
            return family;
        }

        private void CreateProductsSamples(ApplicationDbContext context)
        {
            if (context.Products.Any())
                return;
            Product pain = new Product();
            pain.Name = "Pain complet";
            pain.Description = "Pain farine complete T80";
            pain.Labels.Add(Product.Label.Ab);
            pain.PicturesSerialized = Path.Combine("pain.png");
            pain.Price = Convert.ToDecimal(15.5);
            pain.UnitPrice = 4;
            pain.TaxEnum = Product.TAX.Ten;
            pain.Producer = context.Producers.First();
            pain.ProductUnit = Product.Unit.Kg;
            pain.StockManagement = Product.StockType.Week;
            pain.RemainingStock = 10;
            pain.State = Product.ProductState.Enabled;
            pain.Type = Product.SellType.Piece;
            pain.WeekStock = 10;
            pain.Familly = context.ProductFamillys.First(x => x.FamillyName == "Pains");
            context.Add(pain);
            Product tomate = new Product();
            tomate.Name = "Tomates grappe";
            tomate.Description = "Avec ces tomates, c'est nous qui rougissons même si elles ne sont pas toutes nues!";
            tomate.Labels.Add(Product.Label.Ab);
            tomate.PicturesSerialized = Path.Combine("tomate.jpg");
            tomate.Price = 3;
            tomate.TaxEnum = Product.TAX.FiveFive;
            tomate.UnitPrice = Convert.ToDecimal(1.5);
            tomate.QuantityStep = 500;
            tomate.Producer = context.Producers.First();
            tomate.ProductUnit = Product.Unit.Kg;
            tomate.StockManagement = Product.StockType.Week;
            tomate.RemainingStock = 10;
            tomate.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            tomate.State = Product.ProductState.Enabled;
            tomate.Type = Product.SellType.Weigh;
            tomate.WeekStock = 10;
            context.Add(tomate);
            Product pommedeterre = new Product();
            pommedeterre.Name = "Pomme de terre";
            pommedeterre.Description = "Pataaaaaaaaaaaaaaaates!!";
            pommedeterre.Labels.Add(Product.Label.Ab);
            pommedeterre.PicturesSerialized = Path.Combine("pommedeterre.jpg");
            pommedeterre.Price = Convert.ToDecimal(1.99);
            pommedeterre.TaxEnum = Product.TAX.FiveFive;
            pommedeterre.UnitPrice = Convert.ToDecimal(1.99);
            pommedeterre.QuantityStep = 1000;
            pommedeterre.Producer = context.Producers.First();
            pommedeterre.ProductUnit = Product.Unit.Kg;
            pommedeterre.StockManagement = Product.StockType.Week;
            pommedeterre.RemainingStock = 10;
            pommedeterre.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            pommedeterre.State = Product.ProductState.Enabled;
            pommedeterre.Type = Product.SellType.Weigh;
            pommedeterre.WeekStock = 10;
            context.Add(pommedeterre);
            Product radis = new Product();
            radis.Name = "Radis";
            radis.Description = "Des supers radis (pour ceux qui aiment)";
            radis.Labels.Add(Product.Label.Ab);
            radis.PicturesSerialized = Path.Combine("radis.jpg");
            radis.Price = 0;
            radis.UnitPrice = 4;
            radis.TaxEnum = Product.TAX.FiveFive;
            radis.Producer = context.Producers.First();
            radis.ProductUnit = Product.Unit.Kg;
            radis.StockManagement = Product.StockType.Week;
            radis.RemainingStock = 10;
            radis.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            radis.State = Product.ProductState.Enabled;
            radis.Type = Product.SellType.Piece;
            radis.WeekStock = 10;
            context.Add(radis);
            Product salade = new Product();
            salade.Name = "Salade";
            salade.Description = "Une bonne salade pour aller avec les bonnes tomates!";
            salade.Labels.Add(Product.Label.Ab);
            salade.PicturesSerialized = Path.Combine("salade.jpg");
            salade.UnitPrice = Convert.ToDecimal(0.80);
            salade.TaxEnum = Product.TAX.FiveFive;
            salade.Price = 0;
            salade.Producer = context.Producers.First();
            salade.ProductUnit = Product.Unit.Kg;
            salade.StockManagement = Product.StockType.Week;
            salade.RemainingStock = 10;
            salade.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            salade.State = Product.ProductState.Enabled;
            salade.Type = Product.SellType.Piece;
            salade.WeekStock = 10;
            context.Add(salade);
            Product conserveTomate = new Product();
            conserveTomate.Name = "Bocaux de tomate 500ml";
            conserveTomate.Description = "Bocaux de tomate du jardin, cuillie mur et transformé dans la semaine. Bocaux en verre d'une contenance de 500ml";
            conserveTomate.PicturesSerialized = Path.Combine("ConserveTomate.jpg");
            conserveTomate.UnitPrice = Convert.ToDecimal(4);
            conserveTomate.TaxEnum = Product.TAX.None;
            conserveTomate.Price = 0;
            conserveTomate.Producer = context.Producers.First();
            conserveTomate.ProductUnit = Product.Unit.L;
            conserveTomate.StockManagement = Product.StockType.Fixed;
            conserveTomate.RemainingStock = 10;
            conserveTomate.Familly = context.ProductFamillys.First(x => x.FamillyName == "Légumes");
            conserveTomate.State = Product.ProductState.Enabled;
            conserveTomate.Type = Product.SellType.Piece;
            conserveTomate.WeekStock = 0;
            context.Add(conserveTomate);


            //
            context.SaveChanges();
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            List<string> roleNames = new List<string>();
            //Adding Role
            roleNames.AddRange(Configurations.GetRoles());
            //Adding UserType
            roleNames.AddRange(Configurations.GetUserTypes());
            IdentityResult roleResult;
            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }


        private List<Stolon> CreateStolons(ApplicationDbContext context)
        {
            if (context.Stolons.Any())
                return context.Stolons.ToList();
            List<Stolon> stolons = new List<Stolon>();
            Stolon stolon = new Stolon();
            stolon.IsInMaintenance = false;
            stolon.IsModeSimulated = false;

            stolon.Label = "Stolons de Privas";
            stolon.AboutText = "Les Stolons de Privas est une association loi 1901";
            stolon.JoinUsText = "Pour nous rejoindre contacter nous ! Ou venez nous rendre visite";
            stolon.Address = "07000 PRIVAS";
            stolon.PhoneNumber = "06 64 86 66 93";
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

            stolon.OrderDeliveryMessage = "Votre panier est disponible jeudi de 17h30 à 19h au : 10 place de l'hotel de ville, 07000 Privas";

            stolon.UseProducersFee = true;
            stolon.ProducersFee = 5;

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

        private async Task CreateAdminAccount(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Stolon stolon)
        {
            await CreateAcount(context,
                    userManager,
                    "PARAVEL",
                    "Damien",
                    "damien.paravel@gmail.com",
                    "damien.paravel@gmail.com",
                    Configurations.Role.Administrator,
                    Configurations.UserType.Consumer,
                    stolon);
            await CreateAcount(context,
                    userManager,
                    "MICHON",
                    "Nicolas",
                    "nicolas.michon@zoho.com",
                    "nicolas.michon@zoho.com",
                    Configurations.Role.Administrator,
                    Configurations.UserType.Consumer,
                    stolon);
            await CreateAcount(context,
                    userManager,
                    "TESTON",
                    "Arnaud",
                    "arnaudteston@gmail.com",
                    "arnaudteston@gmail.com",
                    Configurations.Role.Administrator,
                    Configurations.UserType.Consumer,
                    stolon);
        }

        private async Task CreateTestAcount(ApplicationDbContext context, UserManager<ApplicationUser> userManager, Stolon stolon)
        {
            await CreateAcount(context,
                    userManager,
                    "Maurice",
                    "Robert",
                    "chavrouxfdg@hotmail.com",
                    "chavrouxfdg@hotmail.com",
                    Configurations.Role.User,
                    Configurations.UserType.Producer,
                    stolon,
                    true);
        }

        private async Task CreateAcount(ApplicationDbContext context, UserManager<ApplicationUser> userManager, string name, string surname, string email, string password, Configurations.Role role, Configurations.UserType userType, Stolon stolon, bool createOnlyIfTableEmpty = false)
        {
            if (createOnlyIfTableEmpty)
            {
                switch (userType)
                {
                    case Configurations.UserType.Producer:
                        if (context.Producers.Any())
                            return;
                        break;
                    case Configurations.UserType.Consumer:
                        if (context.Consumers.Any())
                            return;
                        break;
                    default:
                        break;
                }
            }

            if (context.Consumers.Any(x => x.Email == email) || context.Producers.Any(x => x.Email == email))
                return;
            StolonsUser user;
            switch (userType)
            {
                case Configurations.UserType.Producer:
                    user = new Producer();
                    break;
                case Configurations.UserType.Consumer:
                    user = new Consumer();
                    break;
                default:
                    user = new Consumer();
                    break;
            }
            user.Name = name;
            user.Surname = surname;
            user.Email = email;
            user.Avatar = Path.Combine(Configurations.UserAvatarStockagePath, Configurations.DefaultFileName);
            user.RegistrationDate = DateTime.Now;
            user.Enable = true;
            user.PostCode = "07000";
            user.Stolon = stolon;

            switch (userType)
            {
                case Configurations.UserType.Producer:
                    Producer producer = user as Producer;
                    producer.CompanyName = "La ferme de " + producer.Name;
                    producer.Latitude = 44.7354673;
                    producer.Longitude = 4.601407399999971;
                    context.Producers.Add(producer);
                    break;
                case Configurations.UserType.Consumer:
                    context.Consumers.Add(user as Consumer);
                    break;
                default:
                    context.Consumers.Add(user as Consumer);
                    break;
            }


            #region Creating linked application data
            var appUser = new ApplicationUser { UserName = user.Email, Email = user.Email };
            appUser.User = user;

            var result = await userManager.CreateAsync(appUser, password);
            if (result.Succeeded)
            {
                //Add user role
                result = await userManager.AddToRoleAsync(appUser, role.ToString());
                //Add user type
                result = await userManager.AddToRoleAsync(appUser, userType.ToString());
            }
            #endregion Creating linked application data

            context.SaveChanges();

        }

        #endregion Stolons config
    }
}
