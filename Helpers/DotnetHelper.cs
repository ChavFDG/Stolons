using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Stolons.Models;

namespace Stolons.Helpers
{
    public static class DotnetHelper
    {

	//Create a new Dependency Injection (DI) scpope for use outside standard dotnet DI contexts
	public static IServiceScope getScope(IWebHost host)
	{
	    var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();

	    return scopeFactory.CreateScope();
	}

	//Get an instance of a db context for the given DI scope
	public static ApplicationDbContext getDbContext(IServiceScope scope)
	{
	    var services = scope.ServiceProvider;

	    return services.GetRequiredService<ApplicationDbContext>();
	}
    }
}
