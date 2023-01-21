using Core.ServiceConfigs;

namespace Core;

public static class BinderCollections
{
    public static Dictionary<string, Binder> Binders { get; set; } = new();
}