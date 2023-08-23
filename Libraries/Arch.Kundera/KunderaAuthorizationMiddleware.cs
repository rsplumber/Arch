using Core.Pipeline.Models;
using FastEndpoints;
using KunderaNet.Services.Authorization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Arch.Kundera;

internal sealed class KunderaAuthorizationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.ProcessorState<RequestState>();
        if (state.EndpointDefinition.AllowAnonymous())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!state.RequestInfo.HasAuthorizationHeader())
        {
            await context.Response.SendUnauthorizedAsync().ConfigureAwait(false);
            return;
        }

        var allowedPermissions = state.EndpointDefinition.ExtractPermissions();
        var allowedRoles = state.EndpointDefinition.ExtractRoles();
        if (RolesOrPermissionsNotConfigured())
        {
            await context.Response.SendUnauthorizedAsync().ConfigureAwait(false);
            return;
        }

        var serviceSecret = state.EndpointDefinition.ExtractServiceSecret();
        if (string.IsNullOrEmpty(serviceSecret))
        {
            await context.Response.SendUnauthorizedAsync().ConfigureAwait(false);
            return;
        }

        state.RequestInfo.AttachServiceSecretToHeader(serviceSecret);

        var authorizeService = context.Resolve<IAuthorizeService>();
        AuthorizedResponse? authorizedResponse;
        int code;
        var token = state.RequestInfo.GetAuthorizationHeader();
        if (HasOnlyAllowedPermissions())
        {
            (code, authorizedResponse) = await authorizeService.AuthorizePermissionAsync(token,
                    allowedPermissions,
                    state.RequestInfo.Headers)
                .ConfigureAwait(false);
        }
        else if (HasOnlyAllowedRoles())
        {
            (code, authorizedResponse) = await authorizeService.AuthorizeRoleAsync(token,
                    allowedRoles,
                    state.RequestInfo.Headers)
                .ConfigureAwait(false);
        }
        else
        {
            await context.Response.SendMultipleAuthorizationNotSupportedAsync();
            return;
        }

        if (AuthorizationCheckFailed())
        {
            await context.Response.SendAuthorizationFailedAsync(code);
            return;
        }

        state.RequestInfo.AttachTempUserTokenToHeader(serviceSecret,
            authorizedResponse!.UserId,
            authorizedResponse.ServiceId,
            authorizedResponse.ScopeId);

        await next(context).ConfigureAwait(false);
        return;

        bool RolesOrPermissionsNotConfigured() => allowedPermissions.Length == 0 && allowedRoles.Length == 0;

        bool HasOnlyAllowedPermissions() => allowedPermissions.Length > 0 && allowedRoles.Length == 0;

        bool HasOnlyAllowedRoles() => allowedRoles.Length > 0 && allowedPermissions.Length == 0;

        bool AuthorizationCheckFailed() => code != 200 || authorizedResponse is null;
    }
}