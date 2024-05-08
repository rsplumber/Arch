using Arch.Configurations;
using Microsoft.AspNetCore.Builder;
using RateLimit.Cage.MiddleWare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Configuration
{
    public static class BeforeDispatchingOptionsExtension
    {
        public static void UseRateLimit(this BeforeDispatchingOptions beforeDispatchingOptions, Action<RateLimitExecutionOptions> action)
        {
            beforeDispatchingOptions.ApplicationBuilder.UseMiddleware<ChackRateLimitMiddleware>();
        }
    }


}
