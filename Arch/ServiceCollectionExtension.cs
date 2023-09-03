using Arch.Configurations;
using Arch.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch;

public static class ServiceCollectionExtension
{
    public static void AddArch(this IServiceCollection services, Action<ArchOptions> archOptions)
    {
        var optionObject = new ArchOptions
        {
            Services = services
        };
        optionObject.AddCore();
        archOptions.Invoke(optionObject);
    }
}