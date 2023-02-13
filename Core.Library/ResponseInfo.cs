namespace Core.Library;

public class ResponseInfo
{
    public required int Code { get; init; }

    public string Value { get; init; } = string.Empty;
}