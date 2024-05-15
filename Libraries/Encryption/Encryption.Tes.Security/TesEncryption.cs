namespace Encryption.Tes.Security;

internal static class TesEncryption
{
    private const int EncryptionKey = 7; // Shift value for encryption
    private const int DecryptionKey = 3; // Shift value for decryption
    private const int CharOffset = 97; // ASCII value for 'a'

    public static string EncryptN(string input)
    {
        // Create a char array to store the encrypted characters
        var encryptedChars = new char[input.Length];

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (int.TryParse(c.ToString(), out var digit))
            {
                // Calculate the encrypted character
                var encryptedChar = (char)((digit + EncryptionKey) % 10 + CharOffset);
                encryptedChars[i] = encryptedChar;
            }
            else
            {
                encryptedChars[i] = c;
            }
        }

        // Create a string from the char array
        return new string(encryptedChars);
    }

    public static string DecryptN(ReadOnlySpan<char> input)
    {
        Span<int> result = stackalloc int[input.Length];
        var resultIndex = 0;

        foreach (var c in input)
        {
            var charCode = c - CharOffset;
            var decryptedDigit = (charCode - EncryptionKey + 10) % 10;
            result[resultIndex++] = decryptedDigit;
        }

        return string.Join("", result.ToArray());
    }


    public static string Encrypt(string text)
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var expirationTime = (currentTime + 30000) / 1000;

        // Use a Span<char> for efficient character manipulation
        Span<char> encryptedText = stackalloc char[text.Length];
        var index = 0;
        foreach (var c in text)
        {
            var charCode = (c - 32 + expirationTime) % 95 + 32;
            encryptedText[index++] = (char)charCode;
        }

        // EncryptN(reversedTime) remains unchanged
        var encryptedKey = EncryptN(ReverseString(expirationTime.ToString()));

        // Combine the parts without intermediate allocations
        var combinedText = $"{encryptedText.ToString()}:{encryptedKey}:{expirationTime}";
        return combinedText;
    }


    public static string Decrypt(ReadOnlySpan<char> encryptedText)
    {
        if (encryptedText.Length == 0) return string.Empty;

        // Extract parts using indices
        var part1 = encryptedText.Slice(0, 8);
        var part2 = encryptedText.Slice(9, 10);
        var part3 = encryptedText[20..];

        // Decrypt part2 (assuming DecryptN exists)
        var reversedTime = DecryptN(part2.ToString());

        // Check expiration time
        var expirationTime = long.Parse(ReverseString(reversedTime)) * 1000;
        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > expirationTime)
        {
            return "Encryption expired";
        }

        // Decrypt part1 (assuming DecryptKey exists)
        var key = DecryptKey(part3, long.Parse(reversedTime));

        // Build decryptedText using Span<char>
        Span<char> decryptedText = stackalloc char[part1.Length];
        for (var i = 0; i < part1.Length; i++)
        {
            var charCode = (part1[i] - 32 - long.Parse(key)) % 95 + 32;
            if (charCode < 32) charCode += 95;
            decryptedText[i] = (char)charCode;
        }

        return decryptedText.ToString();
    }

    private static string ReverseString(ReadOnlySpan<char> s)
    {
        var left = 0;
        var right = s.Length - 1;
        var charArray = s.ToArray();

        while (left < right)
        {
            (charArray[left], charArray[right]) = (charArray[right], charArray[left]);

            left++;
            right--;
        }

        return new string(charArray);
    }


    private static string DecryptKey(ReadOnlySpan<char> encryptedKey, long reversedTime)
    {
        Span<char> key = stackalloc char[encryptedKey.Length];

        for (var i = 0; i < encryptedKey.Length; i++)
        {
            var c = encryptedKey[i];
            var charCode = (c - 32 - reversedTime) % 95 + 32;
            if (charCode < 32) charCode += 95;
            key[i] = (char)charCode;
        }

        return new string(key);
    }
}