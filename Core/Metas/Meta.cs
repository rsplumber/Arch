using Arch.Core.EndpointDefinitions;
using Arch.Core.ServiceConfigs;

namespace Arch.Core.Metas;

public sealed class Meta : BaseEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; } = default!;

    public string Value { get; set; } = default!;

    public EndpointDefinition? EndpointDefinition { get; set; }

    public ServiceConfig? ServiceConfig { get; set; }

    private bool Equals(Meta other)
    {
        return Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Meta other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }
}