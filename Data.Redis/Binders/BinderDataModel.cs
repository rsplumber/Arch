using Core.Binders.Types;
using Redis.OM.Modeling;

namespace Data.Redis.Binders;

[Document(IndexName = "binders", StorageType = StorageType.Json, Prefixes = new[] {"binder"})]
internal sealed class BinderDataModel
{
    [RedisIdField] [Indexed] public string Id { get; set; } = default!;

    [Indexed] public string Pattern { get; set; }

    [Indexed] public string ApiUrl { get; set; }

    [Indexed] public BinderStatus Status { get; set; }
}