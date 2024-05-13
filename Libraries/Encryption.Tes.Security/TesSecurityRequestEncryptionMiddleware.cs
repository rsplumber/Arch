using System.Security.Cryptography;
using System.Text;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline.Models;
using Microsoft.AspNetCore.Http;

namespace Encryption.Tes.Security;

internal sealed class TesSecurityRequestEncryptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        const string apkMd5 = "a60c6906f98dc4aad77585f5b314e54a";

        if (!context.Request.HasBody() || (context.Request.ContentType() is not RequestInfo.ApplicationJsonContentType and RequestInfo.PlainTextContentType))
        {
            await next(context);
            return;
        }

        if (context.RequestState().RequestInfo.Headers.TryGetValue("version", out string value))
        {
            if (int.Parse(value) < 120)
            {
                await next(context);
                return;
            }
        }

        var requestInfo = context.RequestState().RequestInfo;

        var seed = CalculateSeed();
        var authorizationToken = GetAuthorizationToken();

        var encryptedRequest = await ReadRequestBodyAsync();

        var encKey = HashGenerator.GenerateMd5FromString(apkMd5 + authorizationToken + seed);
        var aesEncryption = new AesEncryption(encKey);
        var decryptedText = aesEncryption.DecryptBase64ToString(encryptedRequest);

        context.Items.Add(TesEncryptionContextKey.EncryptionKey, encKey);

        RefillRequestData();

        await next(context);

        return;

        string GetAuthorizationToken()
        {
            return requestInfo.Headers.TryGetValue("Authorization", out var token) ? token : string.Empty;
        }


        string CalculateSeed()
        {
            context.Request.Headers.TryGetValue("key", out var cipheredKey);
            var cipherKey = cipheredKey.FirstOrDefault() ?? string.Empty;
            return TesEncryption.Decrypt(cipherKey);
        }

        async Task<string> ReadRequestBodyAsync()
        {
            context.Request.EnableBuffering();
            var reader = new StreamReader(context.Request.Body);
            var requestBody = await reader.ReadToEndAsync();
            if (context.Request.Body.CanSeek) context.Request.Body.Seek(0, SeekOrigin.Begin);
            return requestBody;
        }

        void RefillRequestData()
        {
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(decryptedText));
            context.Request.ContentType = RequestInfo.ApplicationJsonContentType;
            context.RequestState().RequestInfo.Headers.Remove("Content-Type");
            context.RequestState().RequestInfo.Headers.Add("Content-Type", RequestInfo.ApplicationJsonContentType);
        }
    }
}

public class Encryption
{
    private const int EncryptionKey = 7; // Shift value for encryption
    private const int DecryptionKey = 3; // Shift value for decryption
    private const int CharOffset = 97; // ASCII value for 'a'

    public static string EncryptN(string input)
    {
        var result = string.Empty;
        foreach (var c in input)
        {
            if (int.TryParse(c.ToString(), out int digit))
            {
                var encryptedChar = (char)((digit + EncryptionKey) % 10 + CharOffset);
                result += encryptedChar;
            }
            else
            {
                // If the input contains non-numeric characters, leave them unchanged
                result += c;
            }
        }

        return result;
    }

    public static string DecryptN(string input)
    {
        var result = string.Empty;
        foreach (var c in input)
        {
            var charCode = c - CharOffset;
            var decryptedDigit = (charCode - EncryptionKey + 10) % 10;
            result += decryptedDigit.ToString();
        }

        return result;
    }

    public static string Encrypt(string text)
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var expirationTime = (currentTime + 30000) / 1000;

        var reversedTime = ReverseString(expirationTime.ToString());
        var key = long.Parse(reversedTime);

        var encryptedText = string.Empty;
        foreach (var c in text)
        {
            var charCode = (c - 32 + key) % 95 + 32;
            encryptedText += (char)charCode;
        }

        var encryptedKey = string.Empty;
        var keyString = key.ToString();
        foreach (var c in keyString)
        {
            var charCode = (c - 32 + key) % 95 + 32;
            encryptedKey += (char)charCode;
        }

        var combinedText = encryptedText + ":" + EncryptN(reversedTime) + ":" + encryptedKey;
        return combinedText;
    }

    public static string Decrypt(string encryptedText)
    {
        var part1 = encryptedText.Substring(0, 8);
        var part2 = encryptedText.Substring(9, 10);
        var part3 = encryptedText.Substring(20, 10);
        var reversedTime = DecryptN(part2);
        var encryptedKey = part3;

        var expirationTime = long.Parse(ReverseString(reversedTime)) * 1000;
        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > expirationTime)
        {
            return "Encryption expired";
        }

        encryptedText = part1;
        var key = DecryptKey(encryptedKey, reversedTime);

        var decryptedText = string.Empty;
        foreach (var c in encryptedText)
        {
            var charCode = (c - 32 - long.Parse(key)) % 95 + 32;
            if (charCode < 32) charCode += 95;
            decryptedText += (char)charCode;
        }

        return decryptedText;
    }


    private static string ReverseString(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    private static string DecryptKey(string encryptedKey, string reversedTime)
    {
        var key = string.Empty;
        foreach (var c in encryptedKey)
        {
            var charCode = (c - 32 - long.Parse(reversedTime)) % 95 + 32;
            if (charCode < 32) charCode += 95;
            key += (char)charCode;
        }

        return key;
    }
}

public class AesEncryption
{
    private readonly byte[] _key; // Your existing AES key (you should securely manage this key)

    public AesEncryption(string key)
    {
        _key = Encoding.UTF8.GetBytes(key);
    }

    public string EncryptStringToBase64(string plainText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.Mode = CipherMode.ECB; // Set the mode to ECB

        var encryptor = aesAlg.CreateEncryptor();

        // Convert plaintext to bytes
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        // Encrypt the plaintext
        byte[] encryptedBytes;
        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(plainBytes, 0, plainBytes.Length);
            }

            encryptedBytes = msEncrypt.ToArray();
        }

        // Convert encrypted bytes to base64 for transmission
        return Convert.ToBase64String(encryptedBytes);
    }

    public string DecryptBase64ToString(string base64CipherText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.Mode = CipherMode.ECB; // Set the mode to ECB


        var decryptor = aesAlg.CreateDecryptor();

        // Convert base64 ciphertext to bytes
        byte[] cipherBytes = Convert.FromBase64String(base64CipherText);

        // Decrypt the ciphertext
        byte[] decryptedBytes;
        using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}