using Core;

namespace Arch.Kundera.Exceptions;

public class KunderaForbiddenException : ArchException
{
    private const int DefaultCode = 403;
    private const string DefaultMessage = "Forbidden";

    public KunderaForbiddenException() : base(DefaultCode, DefaultMessage)
    {
    }
}