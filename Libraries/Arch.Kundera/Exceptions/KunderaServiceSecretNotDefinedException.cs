using Core;

namespace Arch.Kundera.Exceptions;

public class KunderaServiceSecretNotDefinedException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "Authorization: service_secret not defined in ServiceConfig meta";

    public KunderaServiceSecretNotDefinedException() : base(DefaultCode, DefaultMessage)
    {
    }
}