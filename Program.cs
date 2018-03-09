using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Stolons.Tools;
using Stolons.Helpers;

namespace Stolons
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            //Register webhost in Stolons configs
            Configurations.WebHost = host;

            Thread billManager = new Thread(() => BillGenerator.ManageBills());
            billManager.Start();

            var logger = DotnetHelper.GetLogger<Program>();

            using (var scope = DotnetHelper.GetNewScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    Startup.SeedDatabase(services);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            try
            {
                host.Run();
            }
            catch (Exception e)
            {
                logger.LogError("Server is dead, catched exception. Bye cruel world");
                throw e;
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .Build();
    }
}
