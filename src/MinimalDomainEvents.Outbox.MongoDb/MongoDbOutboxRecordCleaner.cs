using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbOutboxRecordCleaner : ICleanupOutboxRecords
{
    private readonly IOutboxRecordCollectionProvider _outboxRecordCollectionProvider;
    private readonly IMongoSessionProvider _sessionProvider;

    public MongoDbOutboxRecordCleaner(IOutboxRecordCollectionProvider outboxRecordCollectionProvider, IMongoSessionProvider sessionProvider)
    {
        _outboxRecordCollectionProvider = outboxRecordCollectionProvider;
        _sessionProvider = sessionProvider;
    }

    public async Task CleanupExpiredOutboxRecords(CancellationToken cancellationToken = default)
    {
        var collection = _outboxRecordCollectionProvider.Provide();

        if (_sessionProvider.Session is not null)
        {
            await collection.DeleteManyAsync(_sessionProvider.Session!, Builders<OutboxRecord>.Filter.Lte(or => or.ExpiresAt, DateTimeOffset.UtcNow), null, cancellationToken);
        }
        else
        {
            await collection.DeleteManyAsync(Builders<OutboxRecord>.Filter.Lte(or => or.ExpiresAt, DateTimeOffset.UtcNow), null, cancellationToken);
        }
    }
}
