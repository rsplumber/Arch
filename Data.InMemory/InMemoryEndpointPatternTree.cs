using Core.EndpointDefinitions;
using Core.EndpointDefinitions.Containers;

namespace Data.InMemory;

internal sealed class InMemoryEndpointPatternTree : IEndpointPatternTree
{
    private static EndpointNode _patternTree = EndpointNode.CreateRoot();

    public ValueTask AddAsync(string url, CancellationToken cancellationToken = default)
    {
        return _patternTree.AppendAsync(url, cancellationToken);
    }

    public ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<(string, object[])> FindAsync(string url, CancellationToken cancellationToken = default)
    {
        return _patternTree.FindAsync(url, cancellationToken);
    }

    public void Add(string url)
    {
        _patternTree.Append(url);
    }


    public (string, object[]) Find(string url)
    {
        return _patternTree.Find(url);
    }

    public void Remove(string urlPattern, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        _patternTree = EndpointNode.CreateRoot();
    }
}