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

        var encKey = HashGenerator.GenerateMd5FromString(authorizationToken + apkMd5 + seed);
        Console.WriteLine(seed);
        Console.WriteLine(authorizationToken);
        Console.WriteLine(encKey);
        context.Items.Add(TesEncryptionContextKey.EncryptionKey, encKey);

        if (context.Request.ContentType() is 
            RequestInfo.UrlEncodedFormDataContentType or 
            RequestInfo.MultiPartFormData)
        {
            await next(context);
            return;
        }


        if (!context.Request.HasBody())
        {
            await next(context);
            return;
        }

        var aesEncryption = new AesEncryption(encKey);
        var decryptedText = aesEncryption.DecryptBase64ToString(encryptedRequest);


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
}