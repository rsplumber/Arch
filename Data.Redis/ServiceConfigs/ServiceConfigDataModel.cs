using Core.Binders;
using Core.ServiceConfigs.Types;
using Redis.OM.Modeling;

namespace Data.Redis.ServiceConfigs;

[Document(IndexName = "serviceConfigs", StorageType = StorageType.Json, Prefixes = new[] {"serviceConfig"})]
internal sealed class ServiceConfigDataModel
{
    [RedisIdField] [Indexed] public string Id { get; set; } = default!;


    [Indexed] public string Name { get; set; }

    [Indexed] public string Secret { get; set; }

    [Indexed] public string BaseUrl { get; set; }

    [Indexed] public ServiceStatus Status { get; set; }

    [Indexed] public IList<Binder> Binders { get; set; }
}