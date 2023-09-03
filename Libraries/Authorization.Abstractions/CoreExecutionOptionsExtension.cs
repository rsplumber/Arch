using Arch.Configurations;

namespace Arch.Authorization.Abstractions;

public static class CoreExecutionOptionsExtension
{
    public static void UseAuthorization(this BeforeDispatchingOptions beforeDispatching, Action<AuthorizationExecutionOptions>? options) => options?.Invoke(new AuthorizationExecutionOptions
    {
        ApplicationBuilder = beforeDispatching.ApplicationBuilder
    });
}