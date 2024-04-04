using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker.Abstractions;
public interface IRetrieveOutboxRecords
{
    Task<IReadOnlyCollection<OutboxRecord>> GetAndMarkAsDispatched(CancellationToken cancellationToken = default);
}