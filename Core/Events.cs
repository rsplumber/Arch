namespace Arch.Core;

public interface IEvent
{
    public Guid Id { get; }

    public string Name { get; }

    public DateTime CreatedDateUtc { get; }
}

public abstract record Event : IEvent
{
    public Guid Id => Guid.NewGuid();

    public abstract string Name { get; }

    public DateTime CreatedDateUtc => DateTime.UtcNow;
}

public abstract record DomainEvent : Event;