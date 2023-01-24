using Microsoft.AspNetCore.Builder;

namespace Arch.Kundera;

public static class ApplicationBuilderExtension
{
    public static void UseKundera(this IApplicationBuilder app)
    {
        app.UseMiddleware<KunderaAuthorizationMiddleware>();
    }
}