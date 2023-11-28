using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Core;

public static class DomainEventTracker
{
    private static readonly AsyncLocal<DomainEventScopeStack> _scopeStack = new();

    public static IDomainEventScope CreateScope()
    {
        var stack = GetOrCreateStack();
        var newScope = new DomainEventScope(stack.Count + 1);
        stack.Push(newScope);
        return newScope;
    }

    internal static void ExitScope(IDomainEventScope scope)
    {
        var stack = GetOrCreateStack();
        var deepestScope = stack.Peek();
        if (deepestScope == null || deepestScope.ID < scope.ID)
            return;

        var poppedScope = stack.Pop();
        while (poppedScope!.ID > scope.ID)
            poppedScope = stack.Pop();
    }

    public static void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        var deepestScopeRef = GetOrCreateStack().Peek();
        deepestScopeRef?.RaiseDomainEvent(domainEvent);
    }

    public static IReadOnlyCollection<IDomainEvent> Peek()
    {
        var deepestScopeRef = GetOrCreateStack().Peek();
        return deepestScopeRef?.Peek() ?? EmptyCollection();
    }

    public static IReadOnlyCollection<IDomainEvent> GetAndClearEvents()
    {
        var deepestScopeRef = GetOrCreateStack().Peek();
        return deepestScopeRef?.GetAndClearEvents() ?? EmptyCollection();
    }

    private static IReadOnlyCollection<IDomainEvent> EmptyCollection() =>
        new List<IDomainEvent>(0).AsReadOnly();

    private static DomainEventScopeStack GetOrCreateStack()
    {
        return _scopeStack.Value ??= new DomainEventScopeStack();
    }
}
