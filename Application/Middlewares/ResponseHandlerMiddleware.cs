namespace Application.Middlewares;

internal class ResponseHandlerMiddleware : IMiddleware
{
    private const string ResponseKey = "arch_response";
    private const string ContentType = "application/json; charset=utf-8";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    private const string IgnoreDispatchKey = "ignore_dispatch";


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        dynamic? endpointDefinition = context.Items[ArchEndpointDefinitionKey];
        if (endpointDefinition is null) return;

        foreach (var meta in endpointDefinition.Meta)
        {
            if (meta.Key != IgnoreDispatchKey) continue;
            await next(context);
            return;
        }

        var response = context.Items[ResponseKey] as ResponseInfo;
        context.Response.ContentType = ContentType;
        context.Response.StatusCode = response!.Code;
        await context.Response.WriteAsync(response.Value);
    }
}