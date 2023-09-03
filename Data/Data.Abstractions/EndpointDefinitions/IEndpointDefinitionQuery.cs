namespace Arch.Data.Abstractions.EndpointDefinitions;

public interface IEndpointDefinitionQuery
{
    ValueTask<EndpointDefinitionQueryResponse> QueryAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record EndpointDefinitionQueryResponse
{
    public Guid Id { get; init; }

    public string Method { get; init; } = default!;

    public string Pattern { get; init; } = default!;

    public string Endpoint { get; init; } = default!;

    public string MapTo { get; init; } = default!;

    public IDictionary<string, string> Meta { get; init; } = new Dictionary<string, string>();
}