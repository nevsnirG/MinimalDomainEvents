using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbOutboxRecordRetriever : IRetrieveOutboxRecords
{
    private readonly IOutboxRecordCollectionProvider _outboxRecordCollectionProvider;
    private readonly MongoClient _mongoClient;

    public MongoDbOutboxRecordRetriever(IOutboxRecordCollectionProvider outboxRecordCollectionProvider, MongoClient mongoClient)
    {
        _outboxRecordCollectionProvider = outboxRecordCollectionProvider;
        _mongoClient = mongoClient;
    }

    public Task<IReadOnlyCollection<OutboxRecord>> GetAndMarkAsDispatched()
    {
        var collection = _outboxRecordCollectionProvider.Provide();
        collection.AsQueryable()
            .Where(or => or.DispatchedAt == null)
            .OrderBy(or => or.EnqueuedAt)
    }
}