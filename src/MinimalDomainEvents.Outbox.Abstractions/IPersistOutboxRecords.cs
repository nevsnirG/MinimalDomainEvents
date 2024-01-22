namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IPersistOutboxRecords
{
    Task PersistBatched(OutboxRecord outboxRecord, CancellationToken cancellationToken = default);
    Task PersistIndividually(IReadOnlyCollection<OutboxRecord> outboxRecords, CancellationToken cancellationToken = default);
}