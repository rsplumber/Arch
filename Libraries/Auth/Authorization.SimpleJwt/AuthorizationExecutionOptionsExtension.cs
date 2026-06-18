using Arch.Authorization.Abstractions;
using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Core.ServiceConfigs.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Authorization.SimpleJwt;

public static class AuthorizationExecutionOptionsExtension
{
    public static void UseSimpleJwt(this AuthorizationExecutionOptions executionOptions, IConfiguration configuration)
    {
        executionOptions.ApplicationBuilder.UseMiddleware<SimpleJwtAuthorizationMiddleware>();

        using var serviceScope = executionOptions.ApplicationBuilder.ApplicationServices
            .GetService<IServiceScopeFactory>()?.CreateScope();

        var serviceConfigRepository = serviceScope!.ServiceProvider.GetRequiredService<IServiceConfigRepository>();

        var archServiceConfig = serviceConfigRepository.FindByNameAsync("arch").Result;
        if (archServiceConfig is null) throw new ServiceConfigNotFoundException();

        archServiceConfig.AddMeta("service_secret", configuration.GetSection("SimpleJwt:Secret").Value ??
                                                     throw new Exception("Enter SimpleJwt:Secret in appsettings.json"));

        if (archServiceConfig.EndpointDefinitions.All(d => d.Pattern != "api/v1/auth/token"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/v1/auth/token",
                Pattern = "api/v1/auth/token",
                MapTo = "api/v1/auth/token",
                Method = HttpMethod.Post,
                Meta = new Dictionary<string, string>
                {
                    { "allow_anonymous", "true" }
                }
            });
        }

        serviceConfigRepository.UpdateAsync(archServiceConfig).Wait();
    }
}
