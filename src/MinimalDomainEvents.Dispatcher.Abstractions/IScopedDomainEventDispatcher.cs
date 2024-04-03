using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;

namespace MinimalDomainEvents.Dispatcher.Abstractions;
/// <summary>
/// Responsible for recording domain events and dispatching them in a scoped context.
/// </summary>
public interface IScopedDomainEventDispatcher : IDisposable
{
    IDomainEventScope? Scope { get; }
    void RaiseDomainEvent(IDomainEvent domainEvent);
    Task DispatchAndClear();
}