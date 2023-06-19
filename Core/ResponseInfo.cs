namespace Core;

public class ResponseInfo
{
    public required int Code { get; init; }

    public dynamic? Value { get; init; }

    public long ResponseTimeMilliseconds { get; init; } = -1;

    public string? ContentType { get; init; } = string.Empty;
}