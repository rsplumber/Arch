using Arch.Configurations;

namespace Encryption.Abstractions;

public static class BeforeDispatchingOptionsExtension
{
    public static void UseRequestEncryption(this BeforeDispatchingOptions beforeDispatchingOptions, Action<RequestEncryptionExecutionOptions> options)
    {
        options.Invoke(new RequestEncryptionExecutionOptions
        {
            ApplicationBuilder = beforeDispatchingOptions.ApplicationBuilder
        });
    }
}