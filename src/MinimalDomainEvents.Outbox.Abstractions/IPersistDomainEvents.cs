using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IPersistDomainEvents
{
    Task Persist(IReadOnlyCollection<IDomainEvent> domainEvents);
}