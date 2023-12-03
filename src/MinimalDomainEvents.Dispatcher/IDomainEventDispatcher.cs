using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;

namespace MinimalDomainEvents.Dispatcher;

public interface IDomainEventDispatcher : IDisposable
{
    IDomainEventScope? Scope { get; }
    void RaiseDomainEvent(IDomainEvent domainEvent);
    Task DispatchAndClear();
}
