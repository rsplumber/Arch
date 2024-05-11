using Microsoft.AspNetCore.Builder;

namespace RateLimit.Configuration
{
    public class RateLimitExecutionOptions
    {
        public IApplicationBuilder ApplicationBuilder { get; init; } = default!;

    }
}
