using Microsoft.Extensions.DependencyInjection;

namespace Arch.EndpointGraph.Abstractions;

public sealed class EndpointGraphOptions
{
    public IServiceCollection Services { get; init; } = default!;
}