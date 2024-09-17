

using Arch.Core;

namespace Encryption.Tes.Security;

public class InvalidKey : ArchException
{
    private const int DefaultCode = 460;
    private const string DefaultMessage = "InvalidKey";

    public InvalidKey() : base(DefaultCode, DefaultMessage)
    {
    }
}