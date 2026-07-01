using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Arch.Core.Pipeline.Models;
using Microsoft.IdentityModel.Tokens;

namespace Arch.Authorization.SimpleJwt;

internal static class RequestInfoExtensions
{
    private const string AuthorizationHeaderKey = "Authorization";
    private const string UserTokenKey = "user_token";
    private const string UserIdTokenKey = "uid_token";
    private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();

    public static string GetAuthorizationHeader(this RequestInfo requestInfo)
    {
        return requestInfo.Headers.TryGetValue(AuthorizationHeaderKey, out var value) ? value : string.Empty;
    }

    public static bool HasAuthorizationHeader(this RequestInfo requestInfo) =>
        !string.IsNullOrEmpty(requestInfo.GetAuthorizationHeader());

    public static (bool isValid, string? userId, string? role) ValidateAndExtract(
        this RequestInfo requestInfo,
        string serviceSecret)
    {
        var authHeader = requestInfo.GetAuthorizationHeader();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..]
            : authHeader;

        try
        {
            var principal = JwtSecurityTokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceSecret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userId = principal.FindFirstValue("id");
            var role = principal.FindFirstValue("role");
            return (!string.IsNullOrEmpty(userId), userId, role);
        }
        catch
        {
            return (false, null, null);
        }
    }

    public static void AttachTempUserTokenToHeader(
        this RequestInfo requestInfo,
        string serviceSecret,
        string userId,
        string role)
    {
        var token = JwtSecurityTokenHandler.WriteToken(JwtSecurityTokenHandler
            .CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim("id", userId),
                    new Claim("role", role),
                ]),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceSecret)),
                    SecurityAlgorithms.HmacSha256Signature)
            }));

        requestInfo.Headers.Add(UserTokenKey, token);
        requestInfo.Headers.Add(UserIdTokenKey, token);
    }
}
