namespace Encryption.Tes.Security.Domain;

public abstract class Entity
{
    private HashSet<IDomainEvent>? _domainEvents;

    protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

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