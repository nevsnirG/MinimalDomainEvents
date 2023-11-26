using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Core;

public static class DomainEventTracker
{
    private static readonly AsyncLocal<Stack<WeakReference<DomainEventScope>>> _scopes = new();

    public static IDomainEventScope CreateScope()
    {
        _scopes.Value ??= new Stack<WeakReference<DomainEventScope>>();
        var newScope = new DomainEventScope();
        _scopes.Value.Push(new WeakReference<DomainEventScope>(newScope, false));
        return newScope;
    }

    internal static void ExitScope()
    {
        // TODO - Technically you could manually dispose of a parent scope while inside of a nested scope.
        // In that case pop until the parent scope that was disposed manually is popped?

        // Technically the scope can be exited because the lifetime of a dependency managing the scope has ended.
        // i.e. the ServiceProvider disposes of all disposable dependencies, one of which manages a scope.
        // In that case, the asynclocal has not been initialized because the disposing of the dependency
        // happens in a different async execution context. In that case, the original execution context has ended already
        // and thus all scopes have implicitly been destroyed already.

        //TODO - Hoe werkt dit met try/finally statements?
        _scopes.Value?.Pop();
    }

    public static void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        var deepestScopeRef = _scopes.Value!.Peek();

        deepestScopeRef.IfRefIsStrong(
            @do: strongRef => strongRef.RaiseDomainEvent(domainEvent),
            @else: () => _scopes.Value.Pop()
            );
    }

    public static IReadOnlyCollection<IDomainEvent> Peek()
    {
        if (_scopes.Value!.TryPeek(out var deepestScopeRef))
        {
            return deepestScopeRef.IfRefIsStrong(
                @do: strongRef => strongRef.Peek(),
                @else: () =>
                {
                    _scopes.Value.Pop();
                    return Peek();
                });
        }
        else
            return EmptyCollection();
    }

    public static IReadOnlyCollection<IDomainEvent> GetAndClearEvents()
    {
        if (_scopes.Value!.TryPeek(out var deepestScopeRef))
        {
            return deepestScopeRef.IfRefIsStrong(
                @do: strongRef => strongRef.GetAndClearEvents(),
                @else: () =>
                {
                    _scopes.Value.Pop();
                    return GetAndClearEvents();
                });
        }
        else
            return EmptyCollection();
    }

    private static IReadOnlyCollection<IDomainEvent> EmptyCollection() =>
        new List<IDomainEvent>(0).AsReadOnly();
}