using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Authorization.SimpleJwt;

internal static class HttpResponseExtensions
{
    private const int ForbiddenCode = 403;
    private const string ForbiddenMessage = "Forbidden";

    public static Task SendAuthorizationFailedAsync(this HttpResponse response, int authorizationCode)
    {
        return authorizationCode switch
        {
            ForbiddenCode => response.SendStringAsync(ForbiddenMessage, ForbiddenCode),
            _ => response.SendUnauthorizedAsync(),
        };
    }
}
