using Core;

namespace Arch.Clerk.Exceptions;

public class AccountingUserNotFoundException : ArchException
{
    private const int DefaultCode = 404;
    private const string DefaultMessage = "Accouting: User not found, add authorization to get user";

    public AccountingUserNotFoundException() : base(DefaultCode, DefaultMessage)
    {
    }
}