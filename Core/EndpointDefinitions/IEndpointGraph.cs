namespace Core.EndpointDefinitions;

public interface IEndpointGraph
{
    ValueTask AddAsync(string url, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default);


    /// <summary>
    /// Finds pattern of url in EndpointTree in a async way
    /// </summary>
    /// <param name="url">requested url</param>
    /// <param name="cancellationToken">cancel search</param>
    /// <returns>returns url pattern</returns>
    ValueTask<(string?, object[])> FindAsync(string url, CancellationToken cancellationToken = default);

    ValueTask ClearAsync(CancellationToken cancellationToken = default);
}