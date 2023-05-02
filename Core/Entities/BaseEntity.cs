namespace Core.Entities;

public abstract class BaseEntity
{
    public BaseEntity()
    {
    }

    private HashSet<IDomainEvent>? _domainEvents;

    public IReadOnlyCollection<IDomainEvent>? DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= new HashSet<IDomainEvent>();
        _domainEvents.Add(domainEvent);
    }

    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents?.Remove(domainEvent);
    }

    public void ClearDomainEvent()
    {
        _domainEvents?.Clear();
    }
}