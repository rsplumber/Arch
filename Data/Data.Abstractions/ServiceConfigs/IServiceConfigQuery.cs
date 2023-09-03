namespace Arch.Data.Abstractions.ServiceConfigs;

public interface IServiceConfigQuery
{
    ValueTask<ServiceConfigQueryResponse> QueryAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record ServiceConfigQueryResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public bool Primary { get; init; } = default!;

    public string BaseUrl { get; init; } = default!;

    public IDictionary<string, string> Meta { get; init; } = new Dictionary<string, string>();
}