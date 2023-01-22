namespace Core.PatternTree;

public interface IEndpointPatternTree
{
    ValueTask AddAsync(string url, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default);

    ValueTask<string> FindAsync(string url, CancellationToken cancellationToken = default);

    void Add(string url);

    string Find(string url);
}