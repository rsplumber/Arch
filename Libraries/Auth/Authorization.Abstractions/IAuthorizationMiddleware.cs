using Arch.Core.EndpointDefinitions;
using Arch.Core.Extensions;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline.Models;
using Microsoft.AspNetCore.Http;

namespace Arch.Authorization.Abstractions;

public abstract class AuthorizationMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.RequestState();
        return InvokeAsync(context, state.EndpointDefinition, state.RequestInfo, next);
    }

    protected abstract Task InvokeAsync(HttpContext context,
        EndpointDefinition endpointDefinition,
        RequestInfo requestInfo,
        RequestDelegate next);
}