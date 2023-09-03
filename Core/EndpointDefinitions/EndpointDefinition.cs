using Arch.Core.EndpointDefinitions.Events;
using Arch.Core.Metas;
using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.Exceptions;

namespace Arch.Core.EndpointDefinitions;

public sealed class EndpointDefinition : BaseEntity
{
    private const string DisableKey = "disable";
    private const string LoggingMetaKey = "logging";

    public Guid Id { get; set; }

    public string Pattern { get; set; } = default!;

    public string MapTo { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public HttpMethod Method { get; set; } = default!;

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

    public void Add(Meta meta)
    {
        if (Has(meta)) return;
        Meta.Add(meta);
        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }

    public void RemoveMeta(string key)
    {
        var selectedMeta = Meta.FirstOrDefault(meta => meta.Key == key);
        if (selectedMeta is null) return;
        Meta.Remove(selectedMeta);
        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }

    public bool Has(Meta meta) => Meta.Any(m => m.Key == meta.Key);

    public void Enable()
    {
        var selectedMeta = Meta.FirstOrDefault(meta => meta.Key == "disabled");
        if (selectedMeta is null) return;
        Meta.Remove(selectedMeta);
        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }


    public void Disable()
    {
        var meta = new Meta
        {
            Key = "disabled",
            Value = "true"
        };
        if (Has(meta)) return;
        Meta.Add(meta);
        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }

    public bool IsDisabled() => Meta.Any(meta => meta.Key == DisableKey);

    public LoggingOptions Logging => new(Meta.FirstOrDefault(meta => meta.Key == LoggingMetaKey));

    public sealed record LoggingOptions
    {
        private const string LoggingJustErrorsMetaValue = "error";

        public LoggingOptions(Meta? loggingMeta)
        {
            if (loggingMeta is null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            JustError = loggingMeta.Value == LoggingJustErrorsMetaValue;
        }

        public bool Enabled { get; private set; }

        public bool JustError { get; private set; }
    }
}