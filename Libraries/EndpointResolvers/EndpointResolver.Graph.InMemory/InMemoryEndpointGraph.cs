using Arch.Core.EndpointResolver;

namespace Arch.EndpointResolver.Graph.InMemory;

internal sealed class InMemoryEndpointGraph : IEndpointGraph, IDisposable
{
    private EndpointNode _patternTree = EndpointNode.CreateRoot();
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);

    public ValueTask AddAsync(string url, CancellationToken cancellationToken = default)
    {
        _rwLock.EnterWriteLock();
        try { _patternTree.Append(url); }
        finally { _rwLock.ExitWriteLock(); }
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default)
    {
        _rwLock.EnterWriteLock();
        try { _patternTree.Remove(urlPattern); }
        finally { _rwLock.ExitWriteLock(); }
        return ValueTask.CompletedTask;
    }

    public ValueTask<(string?, object[])> FindAsync(string url, CancellationToken cancellationToken = default)
    {
        _rwLock.EnterReadLock();
        try { return ValueTask.FromResult(_patternTree.Find(url)); }
        finally { _rwLock.ExitReadLock(); }
    }

    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        _rwLock.EnterWriteLock();
        try { _patternTree = EndpointNode.CreateRoot(); }
        finally { _rwLock.ExitWriteLock(); }
        return ValueTask.CompletedTask;
    }

    public ValueTask RebuildAsync(IReadOnlyCollection<string> endpoints, CancellationToken cancellationToken = default)
    {
        var newTree = EndpointNode.CreateRoot();
        foreach (var endpoint in endpoints)
        {
            newTree.Append(endpoint);
        }

        _rwLock.EnterWriteLock();
        try { _patternTree = newTree; }
        finally { _rwLock.ExitWriteLock(); }
        return ValueTask.CompletedTask;
    }

    public void Dispose() => _rwLock.Dispose();
}
