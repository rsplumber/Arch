using Arch.Configurations;

namespace Encryption.Abstractions;

public static class AfterDispatchingOptionsExtension
{
    public static void UseEncryption(this AfterDispatchingOptions beforeDispatchingOptions, Action<EncryptionExecutionOptions> options)
    {
        options.Invoke(new EncryptionExecutionOptions
        {
            ApplicationBuilder = beforeDispatchingOptions.ApplicationBuilder
        });
    }
}