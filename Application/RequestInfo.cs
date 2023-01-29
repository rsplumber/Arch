namespace Application;

public class RequestInfo
{
    public required string Method { get; init; }

    public required string Path { get; init; }

    public Dictionary<string, string>? Headers { get; init; } = new();

    public string? Body { get; init; }
}