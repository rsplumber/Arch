using Microsoft.Extensions.DependencyInjection;

namespace Arch.Core.EndpointResolver;

public sealed class EndpointGraphOptions
{
    public IServiceCollection Services { get; init; } = default!;
}
