using Arch.Authorization.Abstractions;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using KunderaNet.Services.Authorization.Abstractions;
using Microsoft.AspNetCore.Http;
using EndpointDefinition = Arch.Core.ServiceConfigs.EndpointDefinitions.EndpointDefinition;

namespace Arch.Authorization.Kundera;

internal sealed class KunderaAuthorizationMiddleware : AuthorizationMiddleware
{
    protected override async Task InvokeAsync(HttpContext context, EndpointDefinition endpointDefinition, RequestInfo requestInfo, RequestDelegate next)
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

        var allowedPermissions = endpointDefinition.ExtractPermissions();
        var allowedRoles = endpointDefinition.ExtractRoles();
        if (RolesOrPermissionsNotConfigured())
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

        requestInfo.AttachServiceSecretToHeader(serviceSecret);

        var authorizeService = context.Resolve<IAuthorizeService>();
        AuthorizedResponse? authorizedResponse;
        int code;
        var token = requestInfo.GetAuthorizationHeader();
        if (HasOnlyAllowedPermissions())
        {
            (code, authorizedResponse) = await authorizeService.AuthorizePermissionAsync(token,
                    allowedPermissions,
                    requestInfo.Headers)
                .ConfigureAwait(false);
        }
        else if (HasOnlyAllowedRoles())
        {
            (code, authorizedResponse) = await authorizeService.AuthorizeRoleAsync(token,
                    allowedRoles,
                    requestInfo.Headers)
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

        requestInfo.AttachTempUserTokenToHeader(serviceSecret,
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