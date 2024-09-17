namespace Encryption.Tes.Security;

public class CoreException : Exception
{
    public CoreException(int code, string message, string clientMessage) : base(message)
    {
        Code = code;
        Message = message;
        ClientMessage = clientMessage;
    }

    public int Code { get; }

    public new string Message { get; }

    public string ClientMessage { get; }
}