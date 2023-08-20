namespace Core.Pipeline.Models;

public record RequestState
{
    private const string IgnoreDispatchKey = "ignore_dispatch";

    public RequestEndpointDefinition EndpointDefinition { get; set; } = default!;
    public RequestInfo RequestInfo { get; set; } = default!;

    public ResponseInfo? ResponseInfo { get; set; }

    public Dictionary<string, string> Meta { get; set; } = new();

    public bool IgnoreDispatch() => EndpointDefinition.Meta.TryGetValue(IgnoreDispatchKey, out _);

    public string ExtractApiUrl() => $"{EndpointDefinition.BaseUrl}/{RequestInfo.Path}{RequestInfo.QueryString}";
}