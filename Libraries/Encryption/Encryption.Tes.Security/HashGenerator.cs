using System.Security.Cryptography;
using System.Text;

namespace Encryption.Tes.Security;

public static class HashGenerator
{
   

    public static string GenerateMd5FromString(string input)
    {
        using var md5 = MD5.Create();
        // Convert input string to byte array and compute hash
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = new byte[md5.HashSize / 8]; // Pre-allocate a fixed-size buffer

        md5.TryComputeHash(inputBytes, hashBytes, out _); // Compute hash directly into the buffer

        // Convert byte array to hex string
        var sb = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}