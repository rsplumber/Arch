namespace Arch.Clerk;

public class NoBalanceException : Exception
{
    private const string DefaultMessage = "No balance";

    public NoBalanceException() : base(DefaultMessage)
    {
    }
}