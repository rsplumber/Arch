namespace Core.Middlewares.Exceptions;

public class BaseUrlNotFoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "BaseUrl not found";

    public BaseUrlNotFoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}