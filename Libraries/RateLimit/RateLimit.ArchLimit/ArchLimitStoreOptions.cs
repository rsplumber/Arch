using Microsoft.Extensions.DependencyInjection;
using RateLimit.ArchLimit.Store;
using RateLimit.ArchLimit.Store.InMemory;

namespace RateLimit.ArchLimit;

public class ArchLimitStoreOptions
{
    public IServiceCollection Services { get; init; } = default!;

    public void UseInMemoryStore()
    {
        Services.AddMemoryCache();
        Services.AddSingleton<IArchLimitStore, InMemoryArchLimitStore>();
    }

    public void UseRedisStore()
    {
        throw new NotImplementedException("Redis store is not yet implemented.");
    }
}
