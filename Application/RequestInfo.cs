namespace Application;

public class RequestInfo
{
    public required HttpRequestMethod Method { get; init; } = HttpRequestMethod.UNKNOWN;

    public required string Path { get; init; }

    public Dictionary<string, string>? Headers { get; init; } = new();

    public string? Body { get; init; }
}

public enum HttpRequestMethod
{
    DELETE,
    GET,
    PATCH,
    POST,
    PUT,
    UNKNOWN
}