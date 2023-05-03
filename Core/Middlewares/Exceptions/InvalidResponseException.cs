namespace Core.Middlewares.Exceptions;

public class InvalidResponseException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "Invalid response";

    public InvalidResponseException() : base(DefaultCode, DefaultMessage)
    {
    }
}