using Arch.EndpointGraph.Abstractions;

namespace Arch.EndpointGraph.InMemory;

internal sealed class InMemoryEndpointGraph : IEndpointGraph
{
    private static EndpointNode _patternTree = EndpointNode.CreateRoot();

    public ValueTask AddAsync(string url, CancellationToken cancellationToken = default)
    {
        _patternTree.Append(url);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<(string?, object[])> FindAsync(string url, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_patternTree.Find(url));
    }

    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        _patternTree = EndpointNode.CreateRoot();
        return ValueTask.CompletedTask;
    }
}