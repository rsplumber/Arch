using Microsoft.Extensions.DependencyInjection;

namespace Arch.Authorization.Abstractions;

public sealed class AuthorizationOptions
{
    public IServiceCollection Services { get; init; } = default!;
}