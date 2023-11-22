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
        // In that case pop until the parent scope that was disposed manually is popped. 
        _scopes.Value!.Pop();
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