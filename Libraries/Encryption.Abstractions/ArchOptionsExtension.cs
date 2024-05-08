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

    public static void AddResponseEncryption(this ArchOptions archOptions, Action<EncryptionOptions>? options = null)
    {
        options?.Invoke(new EncryptionOptions
        {
            Services = archOptions.Services
        });
    }

    public static void AddRequestEncryption(this ArchOptions archOptions, Action<EncryptionOptions>? options = null)
    {
        options?.Invoke(new EncryptionOptions
        {
            Services = archOptions.Services
        });
    }
}