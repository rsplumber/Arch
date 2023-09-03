using Arch.Core.EndpointDefinitions;
using Arch.Core.ServiceConfigs;
using Arch.Data.Abstractions;
using Arch.Data.Abstractions.EndpointDefinitions;
using Arch.Data.Abstractions.ServiceConfigs;
using Arch.Data.EF.EndpointDefinitions;
using Arch.Data.EF.ServiceConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.EF;

public static class DataOptionsExtension
{
    public static void UseEntityFramework(this DataOptions dataOptions, Action<DbContextOptionsBuilder> optionsAction)
    {
        dataOptions.Services.AddDbContext<AppDbContext>(optionsAction);
        dataOptions.Services.AddScoped<IServiceConfigRepository, ServiceConfigRepository>();
        dataOptions.Services.AddScoped<IEndpointDefinitionRepository, EndpointDefinitionRepository>();

        dataOptions.Services.AddScoped<IServiceConfigQuery, ServiceConfigQuery>();
        dataOptions.Services.AddScoped<IServiceConfigsQuery, ServiceConfigsQuery>();
        dataOptions.Services.AddScoped<IServiceConfigEndpointsQuery, ServiceConfigEndpointsQuery>();
        dataOptions.Services.AddScoped<IEndpointDefinitionQuery, EndpointDefinitionQuery>();
    }
}