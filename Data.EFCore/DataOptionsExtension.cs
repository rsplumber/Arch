using Core.EndpointDefinitions;
using Core.ServiceConfigs;
using Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.EFCore;

public static class DataOptionsExtension
{
    public static void UseEntityFramework(this DataOptions dataOptions, Action<DbContextOptionsBuilder> optionsAction)
    {
        dataOptions.Services.AddDbContext<AppDbContext>(optionsAction);
        dataOptions.Services.AddScoped<IServiceConfigRepository, ServiceConfigRepository>();
        dataOptions.Services.AddScoped<IEndpointDefinitionRepository, EndpointDefinitionRepository>();
    }
}