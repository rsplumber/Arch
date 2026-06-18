using Arch.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Abstractions;

public static class ArchOptionsExtension
{
    public static void AddEncryption(this ArchOptions archOptions, Action<EncryptionOptions>? options = null)
    {
        archOptions.Services.AddSingleton<RequestEncryptionMiddleware>();
        archOptions.Services.AddSingleton<ResponseEncryptionMiddleware>();
        options?.Invoke(new EncryptionOptions
        {
            Services = archOptions.Services
        });
    }
}
