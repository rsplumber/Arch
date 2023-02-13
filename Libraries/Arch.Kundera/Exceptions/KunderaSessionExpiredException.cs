using Core;

namespace Arch.Kundera.Exceptions;

public class KunderaSessionExpiredException : ArchException
{
    private const int DefaultCode = 440;
    private const string DefaultMessage = "Session expired";

    public KunderaSessionExpiredException() : base(DefaultCode, DefaultMessage)
    {
    }
}