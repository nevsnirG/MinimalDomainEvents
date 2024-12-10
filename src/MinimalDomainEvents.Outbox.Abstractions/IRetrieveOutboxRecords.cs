namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IRetrieveOutboxRecords
{
    Task<IReadOnlyCollection<OutboxRecord>> GetAndMarkAsDispatched(CancellationToken cancellationToken = default);
}