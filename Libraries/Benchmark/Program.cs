// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Encryption.Tes.Security;

Console.WriteLine("Hello, World!");


var summary = BenchmarkRunner.Run<Md5VsSha256>();

[MemoryDiagnoser]
public class Md5VsSha256
{
    private static string test = "lkhnasfknadsf54564";
    
    [Benchmark]
    public string Optimize() => HashGenerator.GenerateMd5FromString(test);

    [Benchmark]
    public string Normal() => GenerateMD5FromString(test);
    
    public static string GenerateMD5FromString(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            // Convert input string to byte array and compute hash
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
    
            // Convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
    
            // Return the MD5 hash
            return sb.ToString();
        }
    }

    
}
