using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;

namespace MinimalDomainEvents.Dispatcher.Abstractions;

public interface IDomainEventDispatcher : IDisposable
{
    IDomainEventScope? Scope { get; }
    void RaiseDomainEvent(IDomainEvent domainEvent);
    Task DispatchAndClear();
}
