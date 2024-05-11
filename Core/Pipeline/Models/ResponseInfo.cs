namespace Arch.Core.Pipeline.Models;

public class ResponseInfo
{
    public required int Code { get; init; }

    public dynamic? Value { get; set; }

    public required long ResponseTimeMilliseconds { get; init; }

    public string? ContentType { get; init; } = string.Empty;

    public required Dictionary<string, string> Headers { get; init; }
}