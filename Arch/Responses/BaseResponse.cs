namespace Arch.Responses;

public sealed class BaseResponse
{
    public Meta Meta { get; init; } = default!;

    public object Data { get; init; } = default!;
}

public sealed class Meta
{
    public int Code { get; set; }

    public string Message { get; internal set; } = string.Empty;
}