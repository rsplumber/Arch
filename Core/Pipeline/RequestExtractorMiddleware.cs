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
        state.Set(definition);
        state.Set(new RequestInfo(method, definition.MapTo, pathParameters, context.Request.ReadQueryString())
        {
            Headers = context.Request.Headers()
        });

        await next(context).ConfigureAwait(false);
    }
}