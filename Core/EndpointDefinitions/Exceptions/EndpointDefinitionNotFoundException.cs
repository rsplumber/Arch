using Core.Library.Exceptions;

namespace Core.EndpointDefinitions.Exceptions;

public class EndpointDefinitionNotFoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "Endpoint not found";

    public EndpointDefinitionNotFoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}