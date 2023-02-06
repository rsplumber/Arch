using Core.Library.Exceptions;

namespace Arch.Kundera;

public class KunderaUnAuthorizedException : ArchException
{
    private const int DefaultCode = 403;
    private const string DefaultMessage = "UnAuthorized, Add Authorzation header with token";

    public KunderaUnAuthorizedException() : base(DefaultCode, DefaultMessage)
    {
    }
}