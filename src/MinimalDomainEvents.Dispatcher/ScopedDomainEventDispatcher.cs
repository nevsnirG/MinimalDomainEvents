using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;

namespace MinimalDomainEvents.Dispatcher;

public abstract class ScopedDomainEventDispatcher : IDomainEventDispatcher, IDisposable
{
    private IDomainEventScope? _scope;

    protected ScopedDomainEventDispatcher()
    {
        _scope = DomainEventTracker.CreateScope();
    }

    public virtual async Task DispatchAndClear()
    {
        var domainEvents = _scope?.GetAndClearEvents();
        await Dispatch(domainEvents ?? new List<IDomainEvent>(0).AsReadOnly());
    }

    protected abstract Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents);

    public void Dispose()
    {
        if (_scope is not null)
        {
            _scope.Dispose();
            _scope = null;
        }
    }
}