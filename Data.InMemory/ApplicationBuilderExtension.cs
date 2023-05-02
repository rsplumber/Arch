using Core.Entities.EndpointDefinitions.Containers;
using Data.Sql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.InMemory;

public static class ApplicationBuilderExtension
{
    public static void UseInMemoryData(this IApplicationBuilder app, IConfiguration configuration)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        var configs = dbContext.ServiceConfigs
            .Include(config => config.Meta)
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .ToList();

        var containerInitializer = serviceScope.ServiceProvider.GetRequiredService<IContainerInitializer>();
        containerInitializer.InitializeAsync(configs).Wait();
    }
}