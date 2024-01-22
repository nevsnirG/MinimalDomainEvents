namespace MinimalDomainEvents.Outbox.Worker.Abstractions;
public interface ICleanupOutboxRecords
{
    Task CleanupExpiredOutboxRecords(CancellationToken cancellationToken = default);
}