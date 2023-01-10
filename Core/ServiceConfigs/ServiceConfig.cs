using Core.Binders;
using Core.ServiceConfigs.Types;

namespace Core.ServiceConfigs;

public class ServiceConfig : Entity
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Secret { get; set; }

    public string BaseUrl { get; set; }

    public ServiceStatus Status { get; set; } = ServiceStatus.Enable;

    public IList<Binder> Binders { get; set; }
}