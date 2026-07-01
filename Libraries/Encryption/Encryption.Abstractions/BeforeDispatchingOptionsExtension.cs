using Arch.Configurations;
using Microsoft.AspNetCore.Builder;

namespace Encryption.Abstractions;

public static class BeforeDispatchingOptionsExtension
{
    public static void UseRequestEncryption(this BeforeDispatchingOptions beforeDispatchingOptions)
    {
        beforeDispatchingOptions.ApplicationBuilder.UseMiddleware<RequestEncryptionMiddleware>();
    }
}
