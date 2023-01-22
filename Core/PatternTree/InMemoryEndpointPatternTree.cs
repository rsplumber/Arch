namespace Core.PatternTree;

public class InMemoryEndpointPatternTree : IEndpointPatternTree
{
    private static readonly EndpointNode PatternTree = EndpointNode.CreateRoot();

    public ValueTask AddAsync(string url, CancellationToken cancellationToken = default)
    {
        return PatternTree.AppendAsync(url, cancellationToken);
    }

    public ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default)
    {
        return PatternTree.RemoveAsync(urlPattern, cancellationToken);
    }

    public ValueTask<string> FindAsync(string url, CancellationToken cancellationToken = default)
    {
        return PatternTree.FindAsync(url, cancellationToken);
    }

    public void Add(string url)
    {
        PatternTree.Append(url);
    }

    public string Find(string url)
    {
        return PatternTree.Find(url);
    }
}