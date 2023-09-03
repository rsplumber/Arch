using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Kundera;

internal static class HttpResponseExtensions
{
    private const int UnAuthorizedCode = 401;
    private const int ForbiddenCode = 403;
    private const string ForbiddenMessage = "Forbidden";
    private const int SessionExpiredCode = 440;
    private const string SessionExpiredMessage = "Session expired";
    private const int MultipleAuthorizationNotSupportedCode = 401;
    private const string MultipleAuthorizationNotSupportedMessage = "Authorization: Multiple authorization types not supported, only use permissions or roles";


    public static Task SendAuthorizationFailedAsync(this HttpResponse response, int authorizationCode)
    {
        return authorizationCode switch
        {
            ForbiddenCode => response.SendStringAsync(ForbiddenMessage, ForbiddenCode),
            UnAuthorizedCode => response.SendUnauthorizedAsync(),
            SessionExpiredCode => response.SendStringAsync(SessionExpiredMessage, SessionExpiredCode),
            _ => response.SendUnauthorizedAsync(),
        };
    }

    public static Task SendMultipleAuthorizationNotSupportedAsync(this HttpResponse response)
        => response.SendStringAsync(MultipleAuthorizationNotSupportedMessage, MultipleAuthorizationNotSupportedCode);
}