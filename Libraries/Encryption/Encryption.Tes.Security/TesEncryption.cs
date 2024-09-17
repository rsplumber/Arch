using System.Text;

namespace Encryption.Tes.Security;

internal static class TesEncryption
{
    private const int EncryptionKey = 7; // Shift value for encryption
    private const int DecryptionKey = 3; // Shift value for decryption
    private const int CharOffset = 97; // ASCII value for 'a'

    public static string EncryptN(string input)
    {
        var result = new StringBuilder();
        foreach (char currentChar in input)
        {
            int digit = char.GetNumericValue(currentChar) is double val && val >= 0 && val <= 9 ? (int)val : -1;

            if (digit >= 0 && digit <= 9)
            {
                char encryptedChar = (char)(((digit + EncryptionKey) % 10) + CharOffset);
                result.Append(encryptedChar);
            }
            else
            {
                // If the input contains non-numeric characters, leave them unchanged
                result.Append(currentChar);
            }
        }

        return result.ToString();
    }

    public static string DecryptN(ReadOnlySpan<char> input)
    {
        var result = new StringBuilder();
        foreach (char currentChar in input)
        {
            int charCode = (int)currentChar - CharOffset;
            int decryptedDigit = (charCode - EncryptionKey + 10) % 10;
            result.Append(decryptedDigit);
        }

        return result.ToString();
    }


    public static string Encrypt(string text)
    {
        // Get current time in milliseconds
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Remove milliseconds and add 30 seconds
        long expirationTime = (currentTime + 30000) / 1000;

        // Convert time to string and reverse it
        string reversedTime = new string(expirationTime.ToString().Reverse().ToArray());

        // Encryption key based on reversed time
        long key = long.Parse(reversedTime);

        // Encrypt the text using the key
        var encryptedText = new StringBuilder();
        foreach (char currentChar in text)
        {
            // Restrict to English characters range (32 to 126)
            long charCode = ((currentChar - 32 + key) % 95) + 32;
            encryptedText.Append((char)charCode);
        }

        // Encrypt the key
        var encryptedKey = new StringBuilder();
        string keyString = key.ToString();
        foreach (char currentChar in keyString)
        {
            // Restrict to English characters range (32 to 126)
            long charCode = ((currentChar - 32 + key) % 95) + 32;
            encryptedKey.Append((char)charCode);
        }

        // Combine encrypted text with expiration timestamp and encrypted key
        return encryptedText + "-:-" + EncryptN(reversedTime) + "-:-" + encryptedKey;
    }

    public static string Decrypt(string encryptedText)
    {
        if (encryptedText.Length == 0) return "InvalidCipher-Empty";
        try
        {
            // Extract expiration timestamp and encrypted key from the encrypted text
            string[] parts = encryptedText.Split("-:-");
            string reversedTime = DecryptN(parts[1]); // Update the encryptionKey and charOffset accordingly
            string encryptedKey = parts[2];

            // Convert time to milliseconds
            long expirationTime = long.Parse(new string(reversedTime.Reverse().ToArray())) * 1000;

            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // Check if the encryption has expired
            if (time > expirationTime)
            {
                return "InvalidCipher-Time";
            }

            // Remove expiration timestamp and encrypted key from the encrypted text
            encryptedText = parts[0];

            // Decrypt the key
            var key = new StringBuilder();
            foreach (char currentChar in encryptedKey)
            {
                // Restrict to English characters range (32 to 126)
                var charCode = ((currentChar - 32 - long.Parse(reversedTime)) % 95) + 32;
                if (charCode < 32) charCode += 95;
                key.Append((char)charCode);
            }

            // Decrypt the text using the key
            var decryptedText = new StringBuilder();
            foreach (char currentChar in encryptedText)
            {
                // Restrict to English characters range (32 to 126)
                var charCode = ((currentChar - 32 - long.Parse(key.ToString())) % 95) + 32;
                if (charCode < 32) charCode += 95;
                decryptedText.Append((char)charCode);
            }

            return decryptedText.ToString();
        }
        catch (Exception e)
        {
            return "InvalidCipher";
        }
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