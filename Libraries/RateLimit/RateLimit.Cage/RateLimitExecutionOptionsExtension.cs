﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using RateLimit.Cage.Extension;
using RateLimit.Cage.MiddleWare;
using RateLimit.Configuration;

namespace RateLimit.Cage;

public static class RateLimitExecutionOptionsExtension
{
    public static void UseCage(this RateLimitExecutionOptions executionOptions, IConfiguration configuration)
    {
        RateLimitDefault.MaxAllowedRequestInWindow = int.Parse(configuration["RateLimitDefault:MaxAllowedRequestInWindow"]);
        RateLimitDefault.WindowsSize = TimeSpan.Parse(configuration["RateLimitDefault:WindowsSize"]);
        RateLimitDefault.Version = int.Parse(configuration["RateLimitDefault:Version"]);
        executionOptions.ApplicationBuilder.UseMiddleware<CheckRateLimitMiddleware>();
    }
}