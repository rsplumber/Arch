namespace Core.RequestDispatcher;

public interface IRequestDispatcher
{
    public ValueTask<object> ExecuteAsync(RequestInfo req);
}