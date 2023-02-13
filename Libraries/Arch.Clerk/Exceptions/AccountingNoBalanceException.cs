using Core;

namespace Arch.Clerk.Exceptions;

public class AccountingNoBalanceException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "Accounting: No balance";

    public AccountingNoBalanceException() : base(DefaultCode, DefaultMessage)
    {
    }
}