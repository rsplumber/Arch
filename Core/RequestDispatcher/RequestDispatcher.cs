namespace Core.RequestDispatcher;

public class RequestDispatcher : IRequestDispatcher
{
    public async ValueTask<object> ExecuteAsync(RequestInfo req)
    {
        var pattern = await BaseTree.BaseTreeNode.FindAsync(req.Path);
        BinderCollections.Binders.TryGetValue(pattern, out var binder);

        throw new NotImplementedException();
    }
}