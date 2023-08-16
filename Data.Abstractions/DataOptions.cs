using Microsoft.Extensions.DependencyInjection;

namespace Data.Abstractions;

public sealed class DataOptions
{
    public IServiceCollection Services { get; init; } = default!;
}