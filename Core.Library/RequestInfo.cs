namespace Core.Library;

public class RequestInfo
{
    public const string ApplicationJsonContentType = "application/json";
    public const string FormDataContentType = "application/x-www-form-urlencoded";
    
    public required string Method { get; init; }

    public required string Path { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();

    public string? Body { get; init; }

    public required string? ContentType { get; init; }

    public string? QueryString { get; init; } = default;
}