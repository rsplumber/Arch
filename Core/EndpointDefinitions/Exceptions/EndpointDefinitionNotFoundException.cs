namespace Core.EndpointDefinitions.Exceptions;

public class EndpointDefinitionNotFoundException : ApplicationException
{
    private const string DefaultMessage = "Endpoint config not found";

    public EndpointDefinitionNotFoundException() : base(DefaultMessage)
    {
    }
}