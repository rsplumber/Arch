using Core;

namespace Application.Middlewares.Exceptions;

public class BaseUrlNotfoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "BaseUrl not found";

    public BaseUrlNotfoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}