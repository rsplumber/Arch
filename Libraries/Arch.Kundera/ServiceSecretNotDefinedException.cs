namespace Arch.Kundera;

public class ServiceSecretNotDefinedException : Exception
{
    private const string DefaultMessage = "service_secret not defined in service meta";

    public ServiceSecretNotDefinedException() : base(DefaultMessage)
    {
    }
}