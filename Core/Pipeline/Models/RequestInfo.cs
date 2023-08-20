namespace Core.Pipeline.Models;

public class RequestInfo
{
    public const string ApplicationJsonContentType = "application/json";
    public const string UrlEncodedFormDataContentType = "application/x-www-form-urlencoded";
    public const string MultiPartFormData = "multipart/form-data";

    public Guid RequestId { get; } = Guid.NewGuid();

    public required HttpMethod Method { get; init; }

    public required string Path { get; init; }

    public Dictionary<string, string> AttachedHeaders { get; } = new();

    public string? QueryString { get; init; }

    public DateTime RequestDateUtc { get; } = DateTime.UtcNow;
}