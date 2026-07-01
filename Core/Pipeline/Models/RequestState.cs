namespace Arch.Core.Pipeline.Models;

public record RequestState
{
    public ResolvedEndpoint Endpoint { get; private set; } = default!;

    public RequestInfo RequestInfo { get; private set; } = default!;

    public ResponseInfo? ResponseInfo { get; private set; }

    public void Set(ResolvedEndpoint endpoint) => Endpoint = endpoint;

    public void Set(RequestInfo requestInfo) => RequestInfo = requestInfo;

    public void Set(ResponseInfo responseInfo) => ResponseInfo = responseInfo;

    public void SetServiceUnavailable(long responseTime) => ResponseInfo = new ResponseInfo
    {
        Code = 503,
        Value = "Service Unavailable",
        ResponseTimeMilliseconds = responseTime,
        Headers = []
    };

    public void SetUnAuthorized(long responseTime) => ResponseInfo = new ResponseInfo
    {
        Code = 401,
        Value = "UnAuthorized",
        ResponseTimeMilliseconds = responseTime,
        Headers = []
    };

    public void SetForbidden(long responseTime) => ResponseInfo = new ResponseInfo
    {
        Code = 403,
        Value = "Forbidden",
        ResponseTimeMilliseconds = responseTime,
        Headers = []
    };

    public void SetServiceTimeOut(long responseTime) => ResponseInfo = new ResponseInfo
    {
        Code = 504,
        Value = "Gateway timeout",
        ResponseTimeMilliseconds = responseTime,
        Headers = []
    };

    public bool IgnoreDispatch() => Endpoint.Service.IgnoreDispatch();

    public bool HasEmptyResponse() => ResponseInfo is null;
}