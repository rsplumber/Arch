using Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Sql;

public static class ServiceCollectionExtension
{
    public static void AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ArchDbContext>(
            builder => builder.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IServiceConfigRepository, ServiceConfigRepository>();
        services.AddScoped<IBinderRepository, BinderRepository>();
        services.AddScoped<IMetaRepository, MetaRepository>();
    }
}