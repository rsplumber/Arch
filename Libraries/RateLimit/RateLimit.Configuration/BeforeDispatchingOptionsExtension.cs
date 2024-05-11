using Arch.Configurations;

namespace RateLimit.Configuration
{
    public static class BeforeDispatchingOptionsExtension
    {
        public static void UseRateLimit(this BeforeDispatchingOptions beforeDispatchingOptions, Action<RateLimitExecutionOptions> options)
        {
            options.Invoke(new RateLimitExecutionOptions
            {
                ApplicationBuilder = beforeDispatchingOptions.ApplicationBuilder
            });
        }
    }
}