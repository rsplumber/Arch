using System.Security.Cryptography;
using System.Text;
using Encryption.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Encryption.Archrypt;

internal sealed class ArchryptHandler : IEncryptionHandler
{
    private readonly byte[] _key;

    public ArchryptHandler(IConfiguration configuration)
    {
        var keyString = configuration.GetValue<string>("Archrypt:Key")
            ?? throw new InvalidOperationException("Archrypt:Key configuration is required");
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
    }

    public string ProviderName => "Archrypt";

    public Task<string> EncryptAsync(string plainText, HttpContext context)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        aes.IV.CopyTo(result, 0);
        cipherBytes.CopyTo(result, aes.IV.Length);

        return Task.FromResult(Convert.ToBase64String(result));
    }

    public Task<string> DecryptAsync(string cipherText, HttpContext context)
    {
        var allBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = allBytes[..16];

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = allBytes[16..];
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Task.FromResult(Encoding.UTF8.GetString(plainBytes));
    }
}
