using Arch.Authorization.Abstractions;
using KunderaNet.Services.Authorization.Http;
using KunderaNet.Services.Authorization.Mock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Authorization.Kundera;

public static class AuthorizationOptionsExtension
{
    public static void UseKundera(this AuthorizationOptions options, IConfiguration configuration)
    {
        options.Services.AddKunderaHttpService(configuration);
        options.Services.AddSingleton<KunderaAuthorizationMiddleware>();
    }

    public static void UseKunderaMock(this AuthorizationOptions options, IConfiguration configuration)
    {
        options.Services.AddKunderaMockService(configuration);
        options.Services.AddSingleton<KunderaAuthorizationMiddleware>();
    }
}