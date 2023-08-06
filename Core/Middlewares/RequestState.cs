namespace Core.Middlewares;

public record RequestState
{
    public RequestEndpointDefinition EndpointDefinition { get; set; } = default!;
    public RequestInfo RequestInfo { get; set; } = default!;

    public ResponseInfo? ResponseInfo { get; set; }

    public Dictionary<string, string> Meta { get; set; } = new();

}