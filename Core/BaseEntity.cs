namespace Arch.Core;

public abstract class BaseEntity
{
    public BaseEntity()
    {
    }

    private HashSet<IEvent>? _domainEvents;

    public IReadOnlyCollection<IEvent>? DomainEvents => _domainEvents;

    protected void AddDomainEvent(IEvent domainEvent)
    {
        _domainEvents ??= new HashSet<IEvent>();
        _domainEvents.Add(domainEvent);
    }

    protected void RemoveDomainEvent(IEvent domainEvent)
    {
        _domainEvents?.Remove(domainEvent);
    }

    public void ClearDomainEvent()
    {
        _domainEvents?.Clear();
    }
}