using Arch.Authorization.Abstractions;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using EndpointDefinition = Arch.Core.ServiceConfigs.EndpointDefinitions.EndpointDefinition;

namespace Arch.Authorization.SimpleJwt;

internal sealed class SimpleJwtAuthorizationMiddleware : AuthorizationMiddleware
{
    protected override async Task InvokeAsync(
        HttpContext context,
        EndpointDefinition endpointDefinition,
        RequestInfo requestInfo,
        RequestDelegate next)
    {
        if (endpointDefinition.AllowAnonymous())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!requestInfo.HasAuthorizationHeader())
        {
            await context.Response.SendUnauthorizedAsync().ConfigureAwait(false);
            return;
        }

        var serviceSecret = endpointDefinition.ExtractServiceSecret();
        if (string.IsNullOrEmpty(serviceSecret))
        {
            await context.Response.SendUnauthorizedAsync().ConfigureAwait(false);
            return;
        }

        var (isValid, userId, role) = requestInfo.ValidateAndExtract(serviceSecret);
        if (!isValid || userId is null)
        {
            await context.Response.SendUnauthorizedAsync().ConfigureAwait(false);
            return;
        }

        var allowedRoles = endpointDefinition.ExtractRoles();
        if (allowedRoles.Length > 0 && (role is null || !allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            await context.Response.SendAuthorizationFailedAsync(403).ConfigureAwait(false);
            return;
        }

        requestInfo.AttachTempUserTokenToHeader(serviceSecret, userId, role ?? string.Empty);
        requestInfo.SetRequestBy(Guid.Parse(userId));
        await next(context).ConfigureAwait(false);
    }
}
