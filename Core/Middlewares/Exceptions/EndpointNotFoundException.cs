namespace Core.Middlewares.Exceptions;

public class EndpointNotFoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "Not found";

    public EndpointNotFoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}