using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Configuration
{
    public class RateLimitOption
    {
        public IServiceCollection Services { get; init; } = default!;
    }
}
