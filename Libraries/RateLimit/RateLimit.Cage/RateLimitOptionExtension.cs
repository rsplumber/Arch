using Microsoft.Extensions.DependencyInjection;
using RateLimit.Cage.Services;
using RateLimit.Configuration;

namespace RateLimit.Cage;

public static class RateLimitOptionExtension
{
    public static void AddCage(this RateLimitOption rateLimitOptions)
    {
        rateLimitOptions.Services.AddScoped<RateLimitServices>();
        rateLimitOptions.Services.AddMemoryCache();
    }

}