namespace Encryption.Tes.Security.Domain;

public interface IDomainEvent
{
    public Guid EventId { get; }

    public DateTime CreatedDateUtc { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime CreatedDateUtc { get; } = DateTime.UtcNow;

    public abstract string Name { get; }
}