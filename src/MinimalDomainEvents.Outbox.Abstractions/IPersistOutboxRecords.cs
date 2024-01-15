namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IPersistOutboxRecords
{
    Task PersistBatched(OutboxRecord outboxRecord);
    Task PersistIndividually(IReadOnlyCollection<OutboxRecord> outboxRecords);
}