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

	    Thread billManager = new Thread(() => BillGenerator.ManageBills(host));
	    billManager.Start();

	    //var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
	    using (var scope = DotnetHelper.getScope(host))
	    {
		var services = scope.ServiceProvider;
		try
		{
		    Startup.SeedDatabase(services);
		}
		catch (Exception ex)
		{
		    var logger = services.GetRequiredService<ILogger<Program>>();
		    logger.LogError(ex, "An error occurred seeding the DB.");
		}
	    }

	    try
	    {
		host.Run();
	    } catch (StackOverflowException e)
	    {
		Console.WriteLine("Catched exception....fck it");
		throw e;
	    }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
	    .UseStartup<Startup>()
	    .Build();
    }
}
