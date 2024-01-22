using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbOutboxRecordPersister : IPersistOutboxRecords
{
    private readonly IOutboxRecordCollectionProvider _outboxRecordCollectionProvider;
    private readonly IMongoSessionProvider _mongoSessionProvider;

    public MongoDbOutboxRecordPersister(IOutboxRecordCollectionProvider outboxRecordCollectionProvider, IMongoSessionProvider transactionProvider)
    {
        _outboxRecordCollectionProvider = outboxRecordCollectionProvider;
        _mongoSessionProvider = transactionProvider;
    }

    public async Task PersistIndividually(IReadOnlyCollection<OutboxRecord> outboxRecords, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxRecords);
        if (outboxRecords.Count == 0)
            return;

        var outboxCollection = GetCollection();
        await PersistIndividually(outboxCollection, outboxRecords, cancellationToken);
    }

    public Task PersistBatched(OutboxRecord outboxRecord, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboxRecord);

        var outboxCollection = GetCollection();

        if (_mongoSessionProvider.Session is not null)
            return outboxCollection.InsertOneAsync(_mongoSessionProvider.Session, outboxRecord, cancellationToken: cancellationToken);
        else
            return outboxCollection.InsertOneAsync(outboxRecord, cancellationToken: cancellationToken);
    }

    private Task PersistIndividually(IMongoCollection<OutboxRecord> outboxCollection, IReadOnlyCollection<OutboxRecord> outboxRecords, CancellationToken cancellationToken)
    {
        var writes = new List<InsertOneModel<OutboxRecord>>(outboxRecords.Count);
        foreach (var outboxRecord in outboxRecords)
        {
            var writeModel = new InsertOneModel<OutboxRecord>(outboxRecord);
            writes.Add(writeModel);
        }

        if (_mongoSessionProvider.Session is not null)
            return outboxCollection.BulkWriteAsync(_mongoSessionProvider.Session, writes, cancellationToken: cancellationToken);
        else
            return outboxCollection.BulkWriteAsync(writes, cancellationToken: cancellationToken);
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
}