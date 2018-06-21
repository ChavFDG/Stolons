using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Stolons.Models;

namespace Stolons.Helpers
{
    public static class DotnetHelper
    {

        //Create a new Dependency Injection (DI) scpope for use outside standard dotnet DI contexts
        public static IServiceScope GetNewScope()
        {
            var scopeFactory = Configurations.WebHost.Services.GetRequiredService<IServiceScopeFactory>();

            return scopeFactory.CreateScope();
        }

        //Get an instance of a db context for the given DI scope
        public static ApplicationDbContext getDbContext(IServiceScope scope)
        {
            var services = scope.ServiceProvider;

            return services.GetRequiredService<ApplicationDbContext>();
        }

        //Utility method to get a logger instance outside DI context
        public static ILogger<T> GetLogger<T>() where T : class
        {
            return Configurations.WebHost.Services.GetRequiredService<ILogger<T>>();
        }

    }
}
