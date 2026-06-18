using Encryption.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Archrypt;

public static class EncryptionOptionsExtension
{
    public static void UseArchrypt(this EncryptionOptions options)
    {
        options.Services.AddSingleton<IEncryptionHandler, ArchryptHandler>();
    }
}
