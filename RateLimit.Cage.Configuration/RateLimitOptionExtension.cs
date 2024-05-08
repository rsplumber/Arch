using Microsoft.Extensions.DependencyInjection;
using RateLimit.Cage.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Configuration
{
    public static class RateLimitOptionExtension
    {
        public static void AddCage(this RateLimitOption rateLimitOptions)
        {
            rateLimitOptions.Services.AddScoped<RateLimitServices>();
            rateLimitOptions.Services.AddMemoryCache();
        }

    }
}
