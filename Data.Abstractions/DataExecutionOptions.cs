namespace Data.Abstractions;

public sealed class DataExecutionOptions
{
    public IServiceProvider ServiceProvider { get; init; } = default!;
}