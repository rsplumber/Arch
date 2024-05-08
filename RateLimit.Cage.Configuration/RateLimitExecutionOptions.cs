using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Configuration
{
    public class RateLimitExecutionOptions
    {
        public IApplicationBuilder ApplicationBuilder { get; init; } = default!;

    }
}
