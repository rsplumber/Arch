using Microsoft.AspNetCore.Builder;
using RateLimit.Cage.MiddleWare;
using RateLimit.Configuration;

namespace RateLimit.Cage;

public static class RateLimitExecutionOptionsExtension
{
    public static void UseCage(this RateLimitExecutionOptions executionOptions )
    {
        executionOptions.ApplicationBuilder.UseMiddleware<ChackRateLimitMiddleware>();
    }
}