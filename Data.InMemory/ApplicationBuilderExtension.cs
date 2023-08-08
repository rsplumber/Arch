using Data.EFCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.InMemory;

public static class ApplicationBuilderExtension
{
    public static void UseInMemoryData(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        var configs = dbContext.ServiceConfigs
            .Include(config => config.Meta)
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .ToList();

        var containerInitializer = serviceScope.ServiceProvider.GetRequiredService<InMemoryContainerInitializer>();
        containerInitializer.InitializeAsync(configs).Wait();
    }
}