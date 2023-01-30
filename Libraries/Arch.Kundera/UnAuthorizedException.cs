namespace Arch.Kundera;

public class UnAuthorizedException : Exception
{
    private const string DefaultMessage = "UnAuthorized";

    public UnAuthorizedException() : base(DefaultMessage)
    {
    }
}