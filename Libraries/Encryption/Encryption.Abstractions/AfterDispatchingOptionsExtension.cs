using Arch.Configurations;

namespace Encryption.Abstractions;

public static class AfterDispatchingOptionsExtension
{
    public static void UseResponseEncryption(this AfterDispatchingOptions afterDispatchingOptions, Action<ResponseEncryptionExecutionOptions> options)
    {
        options.Invoke(new ResponseEncryptionExecutionOptions
        {
            ApplicationBuilder = afterDispatchingOptions.ApplicationBuilder
        });
    }
}