namespace Core.Library;

public class RequestEndpointDefinition
{
    public required string Pattern { get; init; } = default!;

    public required string Endpoint { get; init; } = default!;

    public required string Method { get; init; } = default!;

    public Dictionary<string, string> Meta { get; init; } = new();
}