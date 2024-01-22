using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbOutboxRecordCleaner : ICleanupOutboxRecords
{
    private readonly IOutboxRecordCollectionProvider _outboxRecordCollectionProvider;

    public MongoDbOutboxRecordCleaner(IOutboxRecordCollectionProvider outboxRecordCollectionProvider)
    {
        _outboxRecordCollectionProvider = outboxRecordCollectionProvider;
    }

    public async Task CleanupExpiredOutboxRecords(CancellationToken cancellationToken = default)
    {
        var collection = _outboxRecordCollectionProvider.Provide();
        await collection.DeleteManyAsync(Builders<OutboxRecord>.Filter.Lte(or => or.ExpiresAt, DateTimeOffset.UtcNow), cancellationToken);
    }
}
