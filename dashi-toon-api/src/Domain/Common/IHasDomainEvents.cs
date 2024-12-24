namespace DashiToon.Api.Domain.Common;

public interface IHasDomainEvents
{
    IReadOnlyCollection<BaseEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
