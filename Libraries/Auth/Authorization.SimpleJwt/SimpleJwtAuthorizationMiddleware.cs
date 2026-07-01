using Arch.Authorization.Abstractions;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Authorization.SimpleJwt;

internal sealed class SimpleJwtAuthorizationMiddleware : AuthorizationMiddleware
{
    protected override async Task InvokeAsync(
        HttpContext context,
        ResolvedEndpoint endpoint,
        RequestInfo requestInfo,
        RequestDelegate next)
    {
        if (endpoint.AllowAnonymous())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!requestInfo.HasAuthorizationHeader())
        {
            await context.Response.SendUnauthorizedAsync().ConfigureAwait(false);
            return;
        }

        var serviceSecret = endpoint.ExtractServiceSecret();
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

        var allowedRoles = endpoint.ExtractRoles();
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
