using Arch.Configurations;

namespace Encryption.Abstractions;

public static class BeforeDispatchingOptionsExtension
{
    public static void UseEncryption(this BeforeDispatchingOptions beforeDispatchingOptions, Action<EncryptionExecutionOptions> options)
    {
        options.Invoke(new EncryptionExecutionOptions
        {
            ApplicationBuilder = beforeDispatchingOptions.ApplicationBuilder
        });
    }
}