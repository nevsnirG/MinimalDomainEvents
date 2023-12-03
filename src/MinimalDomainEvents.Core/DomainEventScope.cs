using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Core;

public interface IDomainEventScope : IDisposable
{
    int Id { get; }
    IReadOnlyCollection<IDomainEvent> GetAndClearEvents();
    IReadOnlyCollection<IDomainEvent> Peek();
    void RaiseDomainEvent(IDomainEvent domainEvent);
}

internal sealed record class DomainEventScope : IDomainEventScope
{
    public int Id { get; }
    internal IDomainEventScope? Child { get; set; }

    private List<IDomainEvent> _domainEvents = new();

    internal DomainEventScope(int id)
    {
        Id = id;
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IReadOnlyCollection<IDomainEvent> Peek()
    {
        return _domainEvents.AsReadOnly();
    }

    public IReadOnlyCollection<IDomainEvent> GetAndClearEvents()
    {
        var events = new List<IDomainEvent>(_domainEvents);

        if (Child is not null)
            events.AddRange(Child.GetAndClearEvents());

        _domainEvents = new List<IDomainEvent>();
        return events;
    }

    public void Dispose()
    {
        DomainEventTracker.ExitScope(this);
    }
}
