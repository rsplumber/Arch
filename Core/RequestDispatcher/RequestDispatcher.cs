namespace Core.RequestDispatcher;

public class RequestDispatcher : IRequestDispatcher
{
    public async ValueTask<object> ExecuteAsync(RequestInfo req)
    {
        var pattern = BaseTree.BaseTreeNode.Find(req.Path);
        throw new NotImplementedException();
    }
}