using Core.EndpointDefinitions;

namespace Core.Pipeline.Models;

public record RequestState
{
    public EndpointDefinition EndpointDefinition { get; private set; } = default!;

    public RequestInfo RequestInfo { get; private set; } = default!;

    public ResponseInfo? ResponseInfo { get; private set; }

    public void Set(EndpointDefinition endpointDefinition) => EndpointDefinition = endpointDefinition;

    public void Set(RequestInfo requestInfo) => RequestInfo = requestInfo;

    public void Set(ResponseInfo responseInfo) => ResponseInfo = responseInfo;

    public void SetServiceUnavailableResponse() => ResponseInfo = ResponseInfo.ServiceUnavailable;

    public bool IgnoreDispatch() => EndpointDefinition.ServiceConfig.IgnoreDispatch();

    public bool HasEmptyResponse() => ResponseInfo is null;

    public string ResolveDispatchApiUrl() => $"{EndpointDefinition.ServiceConfig.BaseUrl}/{RequestInfo.Path}";
}