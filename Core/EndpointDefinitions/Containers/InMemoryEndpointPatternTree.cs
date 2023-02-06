namespace Core.EndpointDefinitions.Containers;

public sealed class InMemoryEndpointPatternTree : IEndpointPatternTree
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

    public ValueTask<string> FindAsync(string url, CancellationToken cancellationToken = default)
    {
        return _patternTree.FindAsync(url, cancellationToken);
    }

    public void Add(string url)
    {
        _patternTree.Append(url);
    }


    public string Find(string url)
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