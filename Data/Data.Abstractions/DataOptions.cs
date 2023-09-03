using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.Abstractions;

public sealed class DataOptions
{
    public IServiceCollection Services { get; init; } = default!;
}