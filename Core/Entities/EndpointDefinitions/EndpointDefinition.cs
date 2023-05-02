using Core.Entities.EndpointDefinitions.Events;
using Core.Entities.Metas;
using Core.Entities.ServiceConfigs;
using Core.Entities.ServiceConfigs.Exceptions;

namespace Core.Entities.EndpointDefinitions;

public sealed class EndpointDefinition : BaseEntity
{
    public Guid Id { get; set; }

    public string Pattern { get; set; } = default!;

    public string MapTo { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public string Method { get; set; } = default!;

    public List<Meta> Meta { get; set; } = new();

    public ServiceConfig ServiceConfig { get; set; } = default!;

    public void UpdateMeta(Dictionary<string, string> meta)
    {
        if (ServiceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        Meta.Clear();
        foreach (var (key, value) in meta)
        {
            Meta.Add(new Meta
            {
                Key = key,
                Value = value
            });
        }

        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }
}