using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Arch.Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;

namespace Arch.Authorization.SimpleJwt.Endpoints.Token;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly IServiceConfigRepository _serviceConfigRepository;

    public Endpoint(IServiceConfigRepository serviceConfigRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
    }

    public override void Configure()
    {
        Post("auth/token");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var archConfig = await _serviceConfigRepository.FindByNameAsync("arch");
        if (archConfig is null || !archConfig.Meta.TryGetValue("service_secret", out var secret))
        {
            ThrowError("SimpleJwt is not configured.");
            return;
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(req.ExpiryMinutes);
        var handler = new JwtSecurityTokenHandler();

        var token = handler.WriteToken(handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("id", req.UserId.ToString()),
                new Claim("role", req.Role),
            ]),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256Signature)
        }));

        await HttpContext.Response.SendAsync(new Response { Token = token, ExpiresAt = expiresAt }, cancellation: ct);
    }
}

internal sealed class Request
{
    public Guid UserId { get; init; }
    public string Role { get; init; } = default!;
    public int ExpiryMinutes { get; init; } = 60;
}

internal sealed class Response
{
    public string Token { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Enter UserId");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Enter Role")
            .NotNull().WithMessage("Enter Role");

        RuleFor(x => x.ExpiryMinutes)
            .GreaterThan(0).WithMessage("ExpiryMinutes must be greater than 0");
    }
}
