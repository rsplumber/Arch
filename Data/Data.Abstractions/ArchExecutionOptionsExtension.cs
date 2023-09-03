using Arch.Configurations;

namespace Arch.Data.Abstractions;

public static class ArchExecutionOptionsExtension
{
    public static void UseData(this ArchExecutionOptions executionOptions, Action<DataExecutionOptions>? options) => options?.Invoke(new DataExecutionOptions
    {
        ServiceProvider = executionOptions.ApplicationBuilder.ApplicationServices
    });
}