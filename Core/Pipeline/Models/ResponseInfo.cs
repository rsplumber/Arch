namespace Core.Pipeline.Models;

public class ResponseInfo
{
    public static readonly ResponseInfo ServiceUnavailable = new()
    {
        Code = 400,
        Value = string.Empty,
        ResponseTimeMilliseconds = -1
    };

    public required int Code { get; init; }

    public dynamic? Value { get; init; }

    public required long ResponseTimeMilliseconds { get; init; }

    public string? ContentType { get; init; } = string.Empty;
}