namespace EndpointGraph.Abstractions;

public sealed class EndpointGraphExecutionOptions
{
    public IServiceProvider ServiceProvider { get; init; } = default!;
}