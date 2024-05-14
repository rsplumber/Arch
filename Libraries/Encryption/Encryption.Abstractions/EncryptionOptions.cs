using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Abstractions;

public sealed class EncryptionOptions
{
    public IServiceCollection Services { get; init; } = default!;
}