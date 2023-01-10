using Core.Binder.Types;

namespace Core.Binder;

public class Binder : Entity
{
    public string Id { get; set; }

    public string Pattern { get; set; }

    public string ApiUrl { get; set; }

    public BinderStatus Status { get; set; } = BinderStatus.Enable;
}