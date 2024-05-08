using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RateLimit.MiddleWare;
using RateLimit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Extension
{
    public static class RateLimitDependancy
    {
        public static void CustomRateLimit(this IServiceCollection services , Parameter parameter)
        {

            // here register servicees into contaienr

            services.AddScoped<RateLimitServices>();
            services.AddScoped<Parameter>();
            //services.AddScoped<ChackRateLimitMiddleware>();
            services.AddMemoryCache();
        }
        


        public static void UseCustomRateLimit(this IApplicationBuilder app)
        {
            app.UseMiddleware<ChackRateLimitMiddleware>();
        }
    }
}
