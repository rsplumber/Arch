using Microsoft.AspNetCore.Http;

namespace Encryption.Abstractions;

public interface IEncryptionHandler
{
    string ProviderName { get; }
    Task<string> EncryptAsync(string plainText, HttpContext context);
    Task<string> DecryptAsync(string cipherText, HttpContext context);
}
