using Core.EndpointDefinitions;
using Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.EFCore;

public static class ServiceCollectionExtension
{
    public static void AddData(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextBuilder)
    {
        services.AddDbContext<AppDbContext>(dbContextBuilder);
        services.AddScoped<IServiceConfigRepository, ServiceConfigRepository>();
        services.AddScoped<IEndpointDefinitionRepository, EndpointDefinitionRepository>();
    }
}