using Arch.Configurations;
using Microsoft.AspNetCore.Builder;

namespace Encryption.Abstractions;

public static class AfterDispatchingOptionsExtension
{
    public static void UseResponseEncryption(this AfterDispatchingOptions afterDispatchingOptions)
    {
        afterDispatchingOptions.ApplicationBuilder.UseMiddleware<ResponseEncryptionMiddleware>();
    }
}
