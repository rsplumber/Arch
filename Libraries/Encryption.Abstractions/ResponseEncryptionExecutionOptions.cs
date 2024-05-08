
using Microsoft.AspNetCore.Builder;

namespace Encryption.Abstractions;

public sealed class ResponseEncryptionExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}