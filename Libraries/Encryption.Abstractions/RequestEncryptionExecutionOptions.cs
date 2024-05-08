using Microsoft.AspNetCore.Builder;

namespace Encryption.Abstractions;

public sealed class RequestEncryptionExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}