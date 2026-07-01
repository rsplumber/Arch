namespace Arch.Admin.Services;

/// <summary>Lightweight, UI-facing projection of a service configuration.</summary>
public sealed record ServiceSummary
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public bool Primary { get; init; }
    public bool Disabled { get; init; }
    public IReadOnlyList<string> BaseUrls { get; init; } = [];
    public IReadOnlyDictionary<string, string> Meta { get; init; } = new Dictionary<string, string>();
    public int EndpointCount { get; init; }
    public int DisabledEndpointCount { get; init; }
    public DateTime CreatedAtUtc { get; init; }

    public int ActiveEndpointCount => EndpointCount - DisabledEndpointCount;
}

/// <summary>Lightweight, UI-facing projection of an endpoint definition.</summary>
public sealed record EndpointSummary
{
    public required Guid Id { get; init; }
    public required string Method { get; init; }
    public required string Endpoint { get; init; }
    public required string Pattern { get; init; }
    public required string MapTo { get; init; }
    public bool Disabled { get; init; }
    public IReadOnlyDictionary<string, string> Meta { get; init; } = new Dictionary<string, string>();
}

/// <summary>A service together with its endpoints, used by the detail screen.</summary>
public sealed record ServiceWithEndpoints
{
    public required ServiceSummary Service { get; init; }
    public required IReadOnlyList<EndpointSummary> Endpoints { get; init; }
}

