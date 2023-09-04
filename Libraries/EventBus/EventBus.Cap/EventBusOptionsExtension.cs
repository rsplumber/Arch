using Arch.EventBus.Configurations;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.EventBus.Cap;

public static class EventBusOptionsExtension
{
    public static void UseCap(this EventBusOptions eventBusOptions, Action<CapOptions> capOptions)
    {
        eventBusOptions.Services.AddCap(capOptions);
    }
}