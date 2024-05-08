using Arch.Configurations;

namespace Encryption.Abstractions;

public static class ArchOptionsExtension
{
    public static void AddEncryption(this ArchOptions archOptions, Action<EncryptionOptions>? options = null)
    {
        options?.Invoke(new EncryptionOptions
        {
            Services = archOptions.Services
        });
    }
}