using Arch.Core.Extensions;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Core.Pipeline;

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

        var endpointDefinitionResolver = context.EndpointDefinitionResolver();
        var (definition, pathParameters) = await endpointDefinitionResolver.ResolveAsync(path, method).ConfigureAwait(false);
        if (definition is null || definition.IsDisabled())
        {
            await context.Response.SendNotFoundAsync().ConfigureAwait(false);
            return;
        }

        var state = context.RequestState();
        state.Set(definition);
        state.Set(new RequestInfo(method, definition.MapTo, pathParameters, context.Request.ReadQueryString())
        {
            Headers = context.Request.Headers()
        });

         await next(context).ConfigureAwait(false);
    }
}