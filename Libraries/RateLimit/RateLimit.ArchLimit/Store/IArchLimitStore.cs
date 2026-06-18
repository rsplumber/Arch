using RateLimit.ArchLimit.Models;

namespace RateLimit.ArchLimit.Store;

public interface IArchLimitStore
{
    RateLimitEntry? Get(string key);
    void Set(string key, RateLimitEntry entry, TimeSpan expiration);
}
