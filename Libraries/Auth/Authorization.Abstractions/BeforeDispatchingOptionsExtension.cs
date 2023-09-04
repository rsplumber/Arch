using Arch.Configurations;

namespace Arch.Authorization.Abstractions;

public static class BeforeDispatchingOptionsExtension
{
    public static void UseAuthorization(this BeforeDispatchingOptions beforeDispatchingOptions, Action<AuthorizationExecutionOptions>? options = null) => options?.Invoke(new AuthorizationExecutionOptions
    {
        ApplicationBuilder = beforeDispatchingOptions.ApplicationBuilder
    });
}