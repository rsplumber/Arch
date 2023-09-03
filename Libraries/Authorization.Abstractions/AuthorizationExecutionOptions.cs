using Microsoft.AspNetCore.Builder;

namespace Arch.Authorization.Abstractions;

public sealed class AuthorizationExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}