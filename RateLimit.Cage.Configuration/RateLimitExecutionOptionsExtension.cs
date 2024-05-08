using Microsoft.AspNetCore.Builder;
using RateLimit.Cage.MiddleWare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Configuration
{
    public static class RateLimitExecutionOptionsExtension
    {
        public static void UseCage(this RateLimitExecutionOptions executionOptions )
        {
            executionOptions.ApplicationBuilder.UseMiddleware<ChackRateLimitMiddleware>();
        }
    }
}
