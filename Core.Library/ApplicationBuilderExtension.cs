using Microsoft.AspNetCore.Builder;

namespace Core.Library;

public static class ApplicationBuilderExtension
{
    public static void UseArchMiddleware<TMiddleware>(this IApplicationBuilder app)
        where TMiddleware : ArchMiddleware
    {
        app.UseMiddleware<TMiddleware>();
    }
}