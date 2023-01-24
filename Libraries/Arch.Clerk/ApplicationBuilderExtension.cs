using Microsoft.AspNetCore.Builder;

namespace Arch.Clerk;

public static class ApplicationBuilderExtension
{
    public static void UseClerkAccounting(this IApplicationBuilder app)
    {
        app.UseMiddleware<CheckAccountingMiddleware>();
    }
}