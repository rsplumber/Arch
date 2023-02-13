using Microsoft.AspNetCore.Http;

namespace Core.Library;

public abstract class ArchMiddleware : ArchMiddlewareHandler, IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        InitHandler(context);
        return HandleAsync(context, next);
    }

    public abstract Task HandleAsync(HttpContext context, RequestDelegate next);
}

public abstract class ArchMiddlewareHandler
{
    protected const string RequestInfoKey = "request_info";
    protected const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    protected const string ResponseKey = "arch_response";
    protected const string UserIdKey = "user_id";
    private const string DisableKey = "disable";
    private const string IgnoreDispatchKey = "ignore_dispatch";

    protected void InitHandler(HttpContext context)
    {
        RequestInfo = context.Items[RequestInfoKey] as RequestInfo;
        ResponseInfo = context.Items[ResponseKey] as ResponseInfo;
        EndpointDefinition = context.Items[ArchEndpointDefinitionKey] as RequestEndpointDefinition;
        UserId = context.Items[UserIdKey] as string;
    }

    protected RequestInfo? RequestInfo { get; private set; }

    protected ResponseInfo? ResponseInfo { get; private set; }

    protected RequestEndpointDefinition? EndpointDefinition { get; private set; }

    protected string? UserId { get; private set; }

    protected string? GetMeta(string key)
    {
        if (EndpointDefinition is null) return null;
        EndpointDefinition.Meta.TryGetValue(key, out var value);
        return value;
    }

    protected bool IgnoreDispatch()
    {
        return EndpointDefinition is not null && EndpointDefinition.Meta.TryGetValue(IgnoreDispatchKey, out _);
    }

    protected bool IsDisabled()
    {
        return EndpointDefinition is not null && EndpointDefinition.Meta.TryGetValue(DisableKey, out _);
    }
}