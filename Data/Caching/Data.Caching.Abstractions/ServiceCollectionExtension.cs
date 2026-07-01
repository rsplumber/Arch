using Arch.Data.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.Caching.Abstractions;

public static class DataOptionsExtension
{
    public static void AddCaching(this DataOptions dataOptions, Action<CachingOptions>? options = null)
    {
        dataOptions.Services.AddScoped<RoutingStateEventHandlers>();

        options?.Invoke(new CachingOptions
        {
            Services = dataOptions.Services
        });
    }
}