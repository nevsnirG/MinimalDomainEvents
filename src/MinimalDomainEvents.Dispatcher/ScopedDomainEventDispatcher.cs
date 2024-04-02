using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher;

internal sealed class ScopedDomainEventDispatcher : IScopedDomainEventDispatcher
{
    public IDomainEventScope? Scope => _scope;

    private IDomainEventScope? _scope;

    private readonly IEnumerable<IDispatchDomainEvents> _dispatchers;

    public ScopedDomainEventDispatcher(IEnumerable<IDispatchDomainEvents> dispatchers)
    {
        _scope = DomainEventTracker.CreateScope();
        _dispatchers = dispatchers;
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _scope!.RaiseDomainEvent(domainEvent);
    }

    public async Task DispatchAndClear()
    {
        var domainEvents = _scope!.GetAndClearEvents();

        if (domainEvents is null || domainEvents.Count == 0)
            return;

        foreach (var dispatcher in _dispatchers)
            await dispatcher.Dispatch(domainEvents);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (_scope is not null)
        {
            _scope.Dispose();
            _scope = null;
        }
    }
}