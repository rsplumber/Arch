using System.Security.Cryptography;
using System.Text;

namespace Encryption.Tes.Security;

internal sealed class AesEncryption
{
    private readonly byte[] _key;

    public AesEncryption(string key)
    {
        _key = Encoding.UTF8.GetBytes(key);
    }

    public async ValueTask<string> EncryptAsync(string plainText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.Mode = CipherMode.ECB;
        var encryptor = aesAlg.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        using var msEncrypt = new MemoryStream();
        await using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        await csEncrypt.WriteAsync(plainBytes);
        var encryptedBytes = msEncrypt.ToArray();
        return Convert.ToBase64String(encryptedBytes);
    }

    public async ValueTask<string> DecryptAsync(string base64CipherText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.Mode = CipherMode.ECB;
        var decryptor = aesAlg.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(base64CipherText);
        using var msDecrypt = new MemoryStream(cipherBytes);
        await using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return await srDecrypt.ReadToEndAsync();
    }
}