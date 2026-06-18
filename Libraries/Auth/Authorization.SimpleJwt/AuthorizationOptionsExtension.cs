using Arch.Authorization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Authorization.SimpleJwt;

public static class AuthorizationOptionsExtension
{
    public static void UseSimpleJwt(this AuthorizationOptions options)
    {
        options.Services.AddSingleton<SimpleJwtAuthorizationMiddleware>();
    }
}
