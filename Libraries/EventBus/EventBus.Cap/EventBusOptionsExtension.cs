using DotNetCore.CAP;
using EventBus.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Cap;

public static class EventBusOptionsExtension
{
    public static void UseCap(this EventBusOptions eventBusOptions, Action<CapOptions> capOptions)
    {
        eventBusOptions.Services.AddCap(capOptions);
    }
}