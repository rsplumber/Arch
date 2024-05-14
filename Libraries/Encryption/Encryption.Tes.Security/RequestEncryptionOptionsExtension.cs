using Encryption.Abstractions;
using Microsoft.AspNetCore.Builder;

namespace Encryption.Tes.Security;

public static class RequestEncryptionOptionsExtension
{
    public static void UseTesSecurityEncryption(this RequestEncryptionExecutionOptions options)
    {
        options.ApplicationBuilder.UseMiddleware<TesSecurityRequestEncryptionMiddleware>();
    }
}