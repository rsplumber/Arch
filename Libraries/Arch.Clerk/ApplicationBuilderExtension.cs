using Core.EndpointDefinitions;
using Core.Library;
using Core.Metas;
using Core.ServiceConfigs;
using Data.Sql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Clerk;

public static class ApplicationBuilderExtension
{
    public static void UseClerkAccounting(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseArchMiddleware<CheckAccountingMiddleware>();

        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.ServiceConfigs.Any(config => config.Name == "clerk")) return;
        var serviceConfig = new ServiceConfig
        {
            Name = "clerk",
            Primary = true,
            BaseUrl = "http://localhost:5140"
        };

        dbContext.ServiceConfigs.Add(serviceConfig);
        dbContext.SaveChanges();


        var archServiceConfig = dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .FirstAsync(config => config.Name == "arch").Result;

        archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "endpoint-definitions/{id}/accounting/enable",
            Pattern = "endpoint-definitions/##/accounting/enable",
            Method = "patch",
            Meta = new List<Meta>
            {
                new()
                {
                    Key = "permissions",
                    Value = "arch_endpoint_definition_accounting_enable"
                },
            }
        });

        archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "endpoint-definitions/{id}/accounting/disable",
            Pattern = "endpoint-definitions/##/accounting/disable",
            Method = "patch",
            Meta = new List<Meta>
            {
                new()
                {
                    Key = "permissions",
                    Value = "arch_endpoint_definition_accounting_disable"
                },
            }
        });

        dbContext.ServiceConfigs.Update(archServiceConfig);
        dbContext.SaveChanges();
    }
}