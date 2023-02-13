using Core;

namespace Arch.Kundera.Exceptions;

public class KunderaUnAuthorizedException : ArchException
{
    private const int DefaultCode = 401;
    private const string DefaultMessage = "UnAuthorized";

    public KunderaUnAuthorizedException() : base(DefaultCode, DefaultMessage)
    {
    }
}