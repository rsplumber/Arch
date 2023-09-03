using Arch.Configurations;

namespace Arch.Data.Abstractions;

public static class ArchOptionsExtension
{
    public static void ConfigureData(this ArchOptions archOptions, Action<DataOptions>? options = null) => options?.Invoke(new DataOptions
    {
        Services = archOptions.Services
    });
}