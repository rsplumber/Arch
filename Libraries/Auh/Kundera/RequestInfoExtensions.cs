using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Arch.Core.Pipeline.Models;
using Microsoft.IdentityModel.Tokens;

namespace Arch.Kundera;

internal static class RequestInfoExtensions
{
    private const string AuthorizationHeaderKey = "Authorization";
    private const string UserTokenKey = "user_token";
    private const string UserIdTokenKey = "uid_token";
    private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();


    public static string GetAuthorizationHeader(this RequestInfo requestInfo)
    {
        return requestInfo.Headers.TryGetValue(AuthorizationHeaderKey, out var authorizationValue) ? authorizationValue : string.Empty;
    }

    public static bool HasAuthorizationHeader(this RequestInfo requestInfo) => !string.IsNullOrEmpty(requestInfo.GetAuthorizationHeader());

    public static void AttachServiceSecretToHeader(this RequestInfo requestInfo, string serviceSecret)
    {
        requestInfo.Headers.Remove("service_secret");
        requestInfo.Headers.Add("service_secret", serviceSecret);
    }

    public static void AttachTempUserTokenToHeader(this RequestInfo requestInfo,
        string serviceSecret,
        string userId,
        string? serviceId,
        string? scopeId)
    {
        var token = JwtSecurityTokenHandler.WriteToken(JwtSecurityTokenHandler
            .CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", userId),
                    new Claim("service_id", serviceId ?? string.Empty),
                    new Claim("scope_id", scopeId ?? string.Empty),
                }),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceSecret)), SecurityAlgorithms.HmacSha256Signature)
            }));
        requestInfo.Headers.Add(UserTokenKey, token);
        requestInfo.Headers.Add(UserIdTokenKey, token);
    }
}