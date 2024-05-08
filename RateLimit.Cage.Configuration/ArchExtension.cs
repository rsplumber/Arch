using Arch.Configurations;
using Arch.EndpointGraph.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Configuration
{
    public static class ArchExtension
    {

        public static void UseRateLimit(this ArchOptions archOptions, Action<RateLimitOption> rateLimitOptions)
        {
            var service = archOptions.Services;

            rateLimitOptions.Invoke(new Configuration.RateLimitOption()
            {
                Services = service,
            });

            
        }
    }



}
