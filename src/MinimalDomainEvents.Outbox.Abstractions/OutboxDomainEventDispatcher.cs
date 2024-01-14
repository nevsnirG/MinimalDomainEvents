using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Outbox.Abstractions;
internal sealed class OutboxDomainEventDispatcher : IDispatchDomainEvents
{
    private readonly IPersistDomainEvents _domainEventPersister;

    public OutboxDomainEventDispatcher(IPersistDomainEvents domainEventPersister)
    {
        _domainEventPersister = domainEventPersister;
    }

    public async Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        await _domainEventPersister.Persist(domainEvents);
    }
}