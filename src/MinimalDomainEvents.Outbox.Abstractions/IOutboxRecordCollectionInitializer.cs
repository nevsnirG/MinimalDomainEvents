namespace MinimalDomainEvents.Outbox.Abstractions;

public interface IOutboxRecordCollectionInitializer
{
    Task Initialize(CancellationToken cancellationToken = default);
}
