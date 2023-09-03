using Arch.Configurations;

namespace Arch.Authorization.Abstractions;

public static class CoreOptionsExtension
{
    public static void AddAuthorization(this ArchOptions archOptions, Action<AuthorizationOptions>? options = null) => options?.Invoke(new AuthorizationOptions
    {
        Services = archOptions.Services
    });
}