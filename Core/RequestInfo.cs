namespace Core;

public class RequestInfo
{
    public const string ApplicationJsonContentType = "application/json";
    public const string FormDataContentType = "application/x-www-form-urlencoded";

    public Guid RequestId { get; } = Guid.NewGuid();

    public required string Method { get; init; }

    public required string Path { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new();

    public string? Body { get; init; }

    public required string? ContentType { get; init; }

    public string? QueryString { get; init; }

    public DateTime RequestDateUtc { get; } = DateTime.UtcNow;
}