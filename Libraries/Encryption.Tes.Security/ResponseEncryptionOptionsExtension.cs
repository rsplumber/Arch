using Encryption.Abstractions;
using Microsoft.AspNetCore.Builder;

namespace Encryption.Tes.Security;

public static class ResponseEncryptionOptionsExtension
{
    public static void UseTesSecurityEncryption(this ResponseEncryptionExecutionOptions options)
    {
        options.ApplicationBuilder.UseMiddleware<TesSecurityResponseEncryptionMiddleware>();
    }
}