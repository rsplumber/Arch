namespace Core.Exceptions;

public class ServiceConfigNotFoundException : ApplicationException
{
    private const string DefaultMessage = "Service config not found";

    public ServiceConfigNotFoundException() : base(DefaultMessage)
    {
    }
}