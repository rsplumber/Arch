using Core.EndpointDefinitions;
using Core.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Pipeline;

internal sealed class RequestExtractorMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path();
        var method = context.Request.Method();
        if (string.IsNullOrEmpty(path))
        {
            await context.Response.SendNotFoundAsync();
            return;
        }

        var endpointDefinitionResolver = context.Resolve<IEndpointDefinitionResolver>();
        var (definition, pathParameters) = await endpointDefinitionResolver.ResolveAsync(path, method);
        if (definition is null)
        {
            await context.Response.SendNotFoundAsync();
            return;
        }

        var state = context.ProcessorState<RequestState>();
        state.EndpointDefinition = new RequestEndpointDefinition
        {
            Method = definition.Method,
            Meta = definition.Meta,
            Endpoint = definition.Endpoint,
            Pattern = definition.Pattern,
            BaseUrl = definition.BaseUrl,
            MapTo = definition.MapTo
        };

        state.RequestInfo = new RequestInfo
        {
            Method = method,
            Path = definition.GenerateRequestPath(pathParameters),
            QueryString = context.Request.ReadQueryString()
        };
        await next(context);
    }
}