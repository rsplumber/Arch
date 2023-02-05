using Core.Library.Exceptions;

namespace Arch.Kundera;

public class UnAuthorizedException : ArchException
{
    private const int DefaultCode = 403;
    private const string DefaultMessage = "UnAuthorized";

    public UnAuthorizedException() : base(DefaultCode, DefaultMessage)
    {
    }
}