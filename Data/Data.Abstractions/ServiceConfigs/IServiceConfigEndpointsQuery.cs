namespace Arch.Data.Abstractions.ServiceConfigs;

public interface IServiceConfigEndpointsQuery
{
    ValueTask<List<ServiceConfigEndpointsQueryResponse>> QueryAsync(Guid id, string? endpoint, CancellationToken cancellationToken = default);
}

public sealed record ServiceConfigEndpointsQueryResponse
{
    public Guid Id { get; init; }

    public string Endpoint { get; init; } = default!;

    public string Pattern { get; init; } = default!;

    public string Method { get; init; } = default!;

    public string MapTo { get; init; } = default!;
}