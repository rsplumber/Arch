using Arch.Configurations;

namespace RateLimit.Configuration
{
    public static class ArchExtension
    {
        public static void UseRateLimit(this ArchOptions archOptions, Action<RateLimitOption> rateLimitOptions)
        {
            rateLimitOptions.Invoke(new RateLimitOption
            {
                Services = archOptions.Services,
            });
        }
    }
}