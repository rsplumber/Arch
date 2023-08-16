using Microsoft.Extensions.DependencyInjection;

namespace EndpointGraph.Abstractions;

public sealed class EndpointGraphOptions
{
    public IServiceCollection Services { get; init; } = default!;
}