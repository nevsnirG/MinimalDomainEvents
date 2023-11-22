using Nevsnirg.DomainEvents.Contract;
using Nevsnirg.DomainEvents.Core;

namespace Nevsnirg.DomainEvents.Dispatcher;

public abstract class ScopedDomainEventDispatcher : IDomainEventDispatcher, IDisposable
{
    private IDisposable? _scope;

    protected ScopedDomainEventDispatcher()
    {
        _scope = DomainEventTracker.CreateScope();
    }

    public virtual async Task DispatchAndClear()
    {
        var domainEvents = DomainEventTracker.GetAndClearEvents();
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