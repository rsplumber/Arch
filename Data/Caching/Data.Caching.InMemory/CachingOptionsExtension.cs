using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Data.Caching.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.Caching.InMemory;

public static class CachingOptionsExtension
{
    public static void UseInMemory(this CachingOptions cachingOptions, IConfiguration? configuration = default)
    {
        // The shared, process-wide cache store the resolver reads and the synchronizer writes.
        cachingOptions.Services.AddSingleton<IEndpointDefinitionCache, InMemoryEndpointDefinitionCache>();
        cachingOptions.Services.AddScoped<InMemoryEndpointDefinitionResolver>();
        cachingOptions.Services.Decorate<IEndpointDefinitionResolver, InMemoryEndpointDefinitionResolver>();
    }
}
