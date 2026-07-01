using RateLimit.Configuration;

namespace RateLimit.ArchLimit;

public static class RateLimitOptionExtension
{
    public static void AddArchLimit(this RateLimitOption rateLimitOptions, Action<ArchLimitStoreOptions> storeOptions)
    {
        storeOptions.Invoke(new ArchLimitStoreOptions
        {
            Services = rateLimitOptions.Services
        });
    }
}
