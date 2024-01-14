using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Outbox.Abstractions;
internal sealed class OutboxDomainEventDispatcher : IDispatchDomainEvents
{
    public Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        throw new NotImplementedException();
    }
}
