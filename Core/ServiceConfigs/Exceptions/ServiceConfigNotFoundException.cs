namespace Core.ServiceConfigs.Exceptions;

public sealed class ServiceConfigNotFoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "Service config not found";

    public ServiceConfigNotFoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}