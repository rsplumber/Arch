using Arch.Core.EndpointDefinitions;
using Arch.Data.Caching.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.Caching.InMemory;

public static class CachingOptionsExtension
{
    public static void UseInMemory(this CachingOptions cachingOptions, IConfiguration? configuration = default)
    {
        cachingOptions.Services.AddScoped<InMemoryEndpointDefinitionResolver>();
        cachingOptions.Services.Decorate<IEndpointDefinitionResolver, InMemoryEndpointDefinitionResolver>();
        cachingOptions.Services.AddScoped<EventHandlers>();
    }
}