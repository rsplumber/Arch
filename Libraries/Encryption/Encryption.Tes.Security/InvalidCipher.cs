using Arch.Core;

namespace Encryption.Tes.Security;

public class InvalidCipher : CoreException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "InvalidCipher";
    private const string DefaultClinetMessage = "خطا در دریافت اطلاعات";

    public InvalidCipher(string? message) : base(DefaultCode, message, DefaultClinetMessage)
    {
    }
}