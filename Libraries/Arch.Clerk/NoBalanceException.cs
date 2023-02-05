using Core.Library.Exceptions;

namespace Arch.Clerk;

public class NoBalanceException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "No balance";

    public NoBalanceException() : base(DefaultCode, DefaultMessage)
    {
    }
}