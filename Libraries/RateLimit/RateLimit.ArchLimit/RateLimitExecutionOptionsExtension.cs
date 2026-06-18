using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using RateLimit.ArchLimit.Middleware;
using RateLimit.ArchLimit.Models;
using RateLimit.Configuration;

namespace RateLimit.ArchLimit;

public static class RateLimitExecutionOptionsExtension
{
    public static void UseArchLimit(this RateLimitExecutionOptions executionOptions, IConfiguration configuration)
    {
        ArchLimitDefaults.MaxRequests = int.Parse(configuration["RateLimitDefault:MaxAllowedRequestInWindow"]!);
        ArchLimitDefaults.Window = TimeSpan.Parse(configuration["RateLimitDefault:WindowsSize"]!);
        ArchLimitDefaults.Version = int.Parse(configuration["RateLimitDefault:Version"]!);
        executionOptions.ApplicationBuilder.UseMiddleware<ArchLimitMiddleware>();
    }
}
