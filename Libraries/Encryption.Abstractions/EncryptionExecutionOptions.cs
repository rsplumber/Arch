using Microsoft.AspNetCore.Builder;

namespace Encryption.Abstractions;

public sealed class EncryptionExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}