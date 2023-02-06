using Core.Library.Exceptions;

namespace Core.EndpointDefinitions.Exceptions;

public sealed class NodePathNotFoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "Not found";


    public NodePathNotFoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}