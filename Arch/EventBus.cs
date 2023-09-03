using Arch.Configurations;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;

namespace Arch;

public static class ArchOptionsExtension
{
    public static void ConfigureEventBus(this ArchOptions archOptions, Action<EventBusOptions> eventBusOptions) => eventBusOptions?.Invoke(new EventBusOptions
    {
        Services = archOptions.Services
    });
}

public sealed class EventBusOptions
{
    internal IServiceCollection Services { get; init; } = default!;
}

public static class EventBusOptionsExtension
{
    public static void UseCap(this EventBusOptions eventBusOptions, Action<CapOptions> setupAction)
    {
        eventBusOptions.Services.AddCap(setupAction);
    }
}