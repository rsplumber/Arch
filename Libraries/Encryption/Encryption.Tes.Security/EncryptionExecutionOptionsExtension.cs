using Encryption.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Tes.Security;

public static class EncryptionExecutionOptionsExtension
{
    public static void UseTesSecurityEncryption(this EncryptionOptions options, IConfiguration configuration)
    {
        options.Services.AddSingleton<TesSecurityRequestEncryptionMiddleware>();
        options.Services.AddSingleton<TesSecurityResponseEncryptionMiddleware>();
    }
}