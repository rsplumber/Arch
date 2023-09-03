namespace Arch.Data.Abstractions.ServiceConfigs;

public interface IServiceConfigsQuery
{
    ValueTask<List<ServiceConfigsQueryResponse>> QueryAsync(string? name, CancellationToken cancellationToken = default);
}

public sealed record ServiceConfigsQueryResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public bool Primary { get; init; } = default!;

    public string BaseUrl { get; init; } = default!;
}