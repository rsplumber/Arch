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
        aesAlg.Mode = CipherMode.ECB; // Set the mode to ECB
        var encryptor = aesAlg.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes;
        using (MemoryStream msEncrypt = new MemoryStream())
        {
            await using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                await csEncrypt.WriteAsync(plainBytes, 0, plainBytes.Length);
            }
            encryptedBytes = msEncrypt.ToArray();
        }
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