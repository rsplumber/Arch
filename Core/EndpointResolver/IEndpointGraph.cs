namespace Arch.Core.EndpointResolver;

public interface IEndpointGraph
{
    ValueTask AddAsync(string url, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default);

    ValueTask<(string?, object[])> FindAsync(string url, CancellationToken cancellationToken = default);

    ValueTask ClearAsync(CancellationToken cancellationToken = default);

    ValueTask RebuildAsync(IReadOnlyCollection<string> endpoints, CancellationToken cancellationToken = default);
}
