namespace Core.Middlewares.Exceptions;

public class InvalidRequestException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "Invalid request";

    public InvalidRequestException() : base(DefaultCode, DefaultMessage)
    {
    }
}