using Arch.Core.EndpointDefinitions;

namespace Arch.Core.Pipeline.Models;

public record RequestState
{
    public EndpointDefinition EndpointDefinition { get; private set; } = default!;

    public RequestInfo RequestInfo { get; private set; } = default!;

    public ResponseInfo? ResponseInfo { get; private set; }

    public void Set(EndpointDefinition endpointDefinition) => EndpointDefinition = endpointDefinition;

    public void Set(RequestInfo requestInfo) => RequestInfo = requestInfo;

    public void Set(ResponseInfo responseInfo) => ResponseInfo = responseInfo;

    public void SetServiceUnavailable(long responseTime) => ResponseInfo = new ResponseInfo
    {
        Code = 503,
        Value = "Service Unavailable",
        ResponseTimeMilliseconds = responseTime,
        Headers = new Dictionary<string, string>()
    };

    public void SetUnAuthorized(int code, long responseTime) => ResponseInfo = new ResponseInfo
    {
        Code = code,
        Value = code == 401 ? "UnAuthorized" : "Forbidden",
        ResponseTimeMilliseconds = responseTime,
        Headers = new Dictionary<string, string>()
    };

    public void SetServiceTimeOut(long responseTime) => ResponseInfo = new ResponseInfo
    {
        Code = 504,
        Value = "Gateway timeout",
        ResponseTimeMilliseconds = responseTime,
        Headers = new Dictionary<string, string>()
    };

    public bool IgnoreDispatch() => EndpointDefinition.ServiceConfig.IgnoreDispatch();

    public bool HasEmptyResponse() => ResponseInfo is null;
}