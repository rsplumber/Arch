namespace Core.Middlewares.Exceptions;

public class ContentTypeNotSupportedException : ArchException
{
    private const int DefaultCode = 400;
    private const string DefaultMessage = "ContentType not supported";

    public ContentTypeNotSupportedException() : base(DefaultCode, DefaultMessage)
    {
    }
}