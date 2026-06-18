using Microsoft.Extensions.Caching.Memory;
using RateLimit.ArchLimit.Models;

namespace RateLimit.ArchLimit.Store.InMemory;

public class InMemoryArchLimitStore : IArchLimitStore
{
    private readonly IMemoryCache _cache;

    public InMemoryArchLimitStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public RateLimitEntry? Get(string key)
    {
        _cache.TryGetValue(key, out RateLimitEntry? value);
        return value;
    }

    public void Set(string key, RateLimitEntry entry, TimeSpan expiration)
    {
        _cache.Set(key, entry, new MemoryCacheEntryOptions
        {
            Size = 512,
            Priority = CacheItemPriority.Normal,
            SlidingExpiration = expiration
        });
    }
}
