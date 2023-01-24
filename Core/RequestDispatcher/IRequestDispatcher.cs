namespace Core.RequestDispatcher;

public interface IRequestDispatcher
{
    public ValueTask<string?> ExecuteAsync(RequestInfo req);
}