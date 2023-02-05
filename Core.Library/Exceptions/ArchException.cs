namespace Core.Library.Exceptions;

public class ArchException : ApplicationException
{
    public ArchException(int code, string message) : base(message)
    {
        Code = code;
        Message = message;
    }

    public int Code { get; }

    public new string Message { get; }
}