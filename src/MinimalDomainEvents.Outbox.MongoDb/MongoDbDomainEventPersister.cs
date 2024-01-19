using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbDomainEventPersister : IPersistOutboxRecords
{
    private const string CollectionName = "OutboxRecords";

    private readonly IMongoSessionProvider _mongoSessionProvider;
    private readonly OutboxSettings _outboxSettings;
    private readonly MongoClient _mongoClient;

    public MongoDbDomainEventPersister(IMongoSessionProvider transactionProvider, OutboxSettings outboxSettings, MongoClient mongoClient)
    {
        _mongoSessionProvider = transactionProvider;
        _outboxSettings = outboxSettings;
        _mongoClient = mongoClient;
    }

    public async Task PersistIndividually(IReadOnlyCollection<OutboxRecord> outboxRecords)
    {
        ArgumentNullException.ThrowIfNull(outboxRecords);
        if (outboxRecords.Count == 0)
            return;

        var outboxCollection = GetOutboxCollection();
        await PersistIndividually(outboxCollection, outboxRecords);
    }

    public Task PersistBatched(OutboxRecord outboxRecord)
    {
        ArgumentNullException.ThrowIfNull(outboxRecord);

        var outboxCollection = GetOutboxCollection();

        if (_mongoSessionProvider.Session is not null)
            return outboxCollection.InsertOneAsync(_mongoSessionProvider.Session, outboxRecord);
        else
            return outboxCollection.InsertOneAsync(outboxRecord);
    }

    private Task PersistIndividually(IMongoCollection<OutboxRecord> outboxCollection, IReadOnlyCollection<OutboxRecord> outboxRecords)
    {
        var writes = new List<InsertOneModel<OutboxRecord>>(outboxRecords.Count);
        foreach (var outboxRecord in outboxRecords)
        {
            var writeModel = new InsertOneModel<OutboxRecord>(outboxRecord);
            writes.Add(writeModel);
        }

        if (_mongoSessionProvider.Session is not null)
            return outboxCollection.BulkWriteAsync(_mongoSessionProvider.Session, writes);
        else
            return outboxCollection.BulkWriteAsync(writes);
    }

    private IMongoCollection<OutboxRecord> GetOutboxCollection()
    {
        var database = _mongoClient.GetDatabase(_outboxSettings.DatabaseName);
        return database.GetCollection<OutboxRecord>(CollectionName);
    }
}