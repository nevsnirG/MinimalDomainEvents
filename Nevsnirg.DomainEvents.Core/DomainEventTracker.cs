using Nevsnirg.DomainEvents.Contract;
using static System.Formats.Asn1.AsnWriter;

namespace Nevsnirg.DomainEvents.Core;

public static class DomainEventTracker
{
    private static readonly AsyncLocal<Stack<List<IDomainEvent>>> _scopes = new AsyncLocal<Stack<List<IDomainEvent>>>();

    public static IDomainEventScope CreateScope()
    {
        _scopes.Value ??= new Stack<List<IDomainEvent>>();
        _scopes.Value.Push(new List<IDomainEvent>());

        return new DomainEventScope(_scopes.Value.Count);
    }

    internal static void ExitScope()
    {
        _scopes.Value!.Pop();
    }

    public static void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        var deepestScope = _scopes.Value!.Peek();
        deepestScope.Add(domainEvent);
    }

    public static IReadOnlyCollection<IDomainEvent> Peek()
    {
        if (_scopes.Value!.TryPeek(out var deepestScope))
            return deepestScope!.AsReadOnly();
        else
            return new List<IDomainEvent>().AsReadOnly();
    }

    public static IReadOnlyCollection<IDomainEvent> GetAndClearEvents()
    {
        var deepestScope = _scopes.Value!.Pop();
        _scopes.Value.Push(new List<IDomainEvent>());
        return deepestScope.AsReadOnly() ?? new List<IDomainEvent>().AsReadOnly();
    }
}