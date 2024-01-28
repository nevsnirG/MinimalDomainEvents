using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbOutboxRecordRetriever : IRetrieveOutboxRecords
{
    private readonly IOutboxRecordCollectionProvider _outboxRecordCollectionProvider;
    private readonly ITransactionProvider _transactionProvider;

    public MongoDbOutboxRecordRetriever(IOutboxRecordCollectionProvider outboxRecordCollectionProvider, ITransactionProvider transactionProvider)
    {
        _outboxRecordCollectionProvider = outboxRecordCollectionProvider;
        _transactionProvider = transactionProvider;
    }

    public async Task<IReadOnlyCollection<OutboxRecord>> GetAndMarkAsDispatched(CancellationToken cancellationToken)
    {
        var session = GetCurrentMongoClientSession();
        var collection = GetCollection();

        var outboxRecords = await FindOldestNotDispatchedOutboxRecords(session, collection, cancellationToken);
        await UpdateDispatchedAt(session, collection, outboxRecords, cancellationToken);

        return outboxRecords;
    }

    private IMongoCollection<OutboxRecord> GetCollection()
    {
        var collectionSettings = new MongoCollectionSettings
        {
            ReadConcern = ReadConcern.Majority,
            ReadPreference = ReadPreference.Primary,
            WriteConcern = WriteConcern.WMajority
        };
        return _outboxRecordCollectionProvider.Provide(collectionSettings);
    }

    private IClientSessionHandle GetCurrentMongoClientSession()
    {
        if (!_transactionProvider.TryGetCurrentTransaction(out var transaction) || transaction is not MongoDbOutboxTransaction mongoOutboxTransaction)
            throw new InvalidOperationException("A mongo transaction must have been started.");
        return mongoOutboxTransaction.ClientSessionHandle;
    }

    private static async Task<List<OutboxRecord>> FindOldestNotDispatchedOutboxRecords(IClientSessionHandle session, IMongoCollection<OutboxRecord> collection, CancellationToken cancellationToken)
    {
        return await collection.AsQueryable(session)
                    .Where(or => or.DispatchedAt == null)
                    .OrderBy(or => or.EnqueuedAt)
                    .ToListAsync(cancellationToken);
    }

    private static async Task UpdateDispatchedAt(IClientSessionHandle session, IMongoCollection<OutboxRecord> collection, List<OutboxRecord> outboxRecords, CancellationToken cancellationToken)
    {
        var outboxRecordIds = outboxRecords.Select(or => or.Id);
        var filter = Builders<OutboxRecord>.Filter.In(or => or.Id, outboxRecordIds);
        var update = Builders<OutboxRecord>.Update.Set(or => or.DispatchedAt, DateTimeOffset.UtcNow);
        var updateOptions = new UpdateOptions
        {
            IsUpsert = false
        };
        await collection.UpdateManyAsync(session, filter, update, updateOptions, cancellationToken);
    }
}