namespace Core.ServiceConfigs.Exceptions;

public sealed class PrimaryServiceModificationException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "Cannot modify primary services";

    public PrimaryServiceModificationException() : base(DefaultCode, DefaultMessage)
    {
    }
}