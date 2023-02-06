using Core.Library.Exceptions;

namespace Arch.Kundera;

public class KunderaMultipleAuthorizationTypeException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "Authorization: Multiple authorization types not supported, only use permissions or roles";

    public KunderaMultipleAuthorizationTypeException() : base(DefaultCode, DefaultMessage)
    {
    }
}