namespace Core.Entities.EndpointDefinitions.Exceptions;

public sealed class EndpointDefinitionNotFoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "Endpoint not found";

    public EndpointDefinitionNotFoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}