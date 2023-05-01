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
        if (!dbContext.ServiceConfigs.Any(config => config.Name == "clerk"))
        {
            var serviceConfig = new ServiceConfig
            {
                Name = "clerk",
                Primary = true,
                BaseUrl = configuration.GetSection("Clerk:BaseUrl").Value ??
                          throw new Exception("Enter Clerk:BaseUrl in appsettings.json")
            };

            dbContext.ServiceConfigs.Add(serviceConfig);
            dbContext.SaveChanges();
        }


        var archServiceConfig = dbContext.ServiceConfigs
            .Include(config => config.Meta)
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .First(config => config.Name == "arch");

        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "gateway/api/v1/endpoint-definitions/##/accounting/enable"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "gateway/api/v1/endpoint-definitions/{id}/accounting/enable",
                Pattern = "gateway/api/v1/endpoint-definitions/##/accounting/enable",
                MapTo = "gateway/api/v1/endpoint-definitions/{0}/accounting/enable",
                Method = HttpRequestMethods.Post,
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_endpoint_definition_accounting_enable"
                    },
                }
            });
        }


        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "gateway/api/v1/endpoint-definitions/##/accounting/disable"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "gateway/api/v1/endpoint-definitions/{id}/accounting/disable",
                Pattern = "gateway/api/v1/endpoint-definitions/##/accounting/disable",
                MapTo = "gateway/api/v1/endpoint-definitions/{0}/accounting/disable",
                Method = HttpRequestMethods.Post,
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_endpoint_definition_accounting_disable"
                    },
                }
            });
        }

        dbContext.ServiceConfigs.Update(archServiceConfig);
        dbContext.SaveChanges();
    }
}