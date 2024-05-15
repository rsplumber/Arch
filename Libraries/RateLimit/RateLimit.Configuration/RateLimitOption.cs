using Microsoft.Extensions.DependencyInjection;

namespace RateLimit.Configuration
{
    public class RateLimitOption
    {
        public IServiceCollection Services { get; init; } = default!;
    }
}