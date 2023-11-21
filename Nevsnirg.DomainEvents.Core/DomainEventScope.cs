using Nevsnirg.DomainEvents.Contract;

namespace Nevsnirg.DomainEvents.Core;

public interface IDomainEventScope : IDisposable
{
    int Id { get; } //TODO - Change to Guid/string.
    IReadOnlyCollection<IDomainEvent> GetAndClearEvents();
    void RaiseDomainEvent(IDomainEvent domainEvent);
}

internal sealed class DomainEventScope : IDomainEventScope
{
    public int Id { get; }

    public DomainEventScope(int id)
    {
        Id = id;
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        DomainEventTracker.RaiseDomainEvent(domainEvent);
    }

    public IReadOnlyCollection<IDomainEvent> GetAndClearEvents()
    {
        return DomainEventTracker.GetAndClearEvents();
    }

    public void Dispose()
    {
        DomainEventTracker.ExitScope();
    }
}
