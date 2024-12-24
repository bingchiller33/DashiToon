using System.ComponentModel.DataAnnotations.Schema;

namespace DashiToon.Api.Domain.Common;

public abstract class BaseEntity<T> : IHasDomainEvents
{
    private readonly List<BaseEvent> _domainEvents = new();

    // This can easily be modified to be BaseEntity<T> and public T Id to support different key types.
    // Using non-generic integer types for simplicity
    public T Id { get; set; } = default!;

    [NotMapped] public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }
}
