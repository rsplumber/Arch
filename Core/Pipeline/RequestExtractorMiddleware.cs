using Core.EndpointDefinitions;
using Core.Extensions;
using Core.Pipeline.Models;
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
            await context.Response.SendNotFoundAsync().ConfigureAwait(false);
            return;
        }

        var endpointDefinitionResolver = context.Resolve<IEndpointDefinitionResolver>();
        var (definition, pathParameters) = await endpointDefinitionResolver.ResolveAsync(path, method).ConfigureAwait(false);
        if (definition is null || definition.IsDisabled())
        {
            await context.Response.SendNotFoundAsync().ConfigureAwait(false);
            return;
        }

        var state = context.ProcessorState<RequestState>();
        state.EndpointDefinition = new RequestEndpointDefinition
        {
            Method = definition.Method,
            Meta = definition.Meta.ToDictionary(a => a.Key, a => a.Value),
            Endpoint = definition.Endpoint,
            Pattern = definition.Pattern,
            BaseUrl = definition.ServiceConfig.BaseUrl,
            MapTo = definition.MapTo
        };

        state.RequestInfo = new RequestInfo
        {
            Method = method,
            Path = definition.GenerateRequestPath(pathParameters),
            QueryString = context.Request.ReadQueryString()
        };
        await next(context).ConfigureAwait(false);
    }
}