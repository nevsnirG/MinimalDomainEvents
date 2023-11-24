using MinimalDomainEvents.Core;

namespace MinimalDomainEvents.Dispatcher;

public interface IDomainEventDispatcher : IDisposable
{
    IDomainEventScope? Scope { get; }
    Task DispatchAndClear();
}
