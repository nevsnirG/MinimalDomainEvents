using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

//TODO - Turn transactions on/off.
namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbDomainEventPersister : IPersistOutboxRecords
{
    private const string CollectionName = "OutboxRecords";

    private readonly OutboxSettings _outboxSettings;
    private readonly MongoClient _mongoClient;

    public MongoDbDomainEventPersister(OutboxSettings outboxSettings, MongoClient mongoClient)
    {
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

    public async Task PersistBatched(OutboxRecord outboxRecord)
    {
        ArgumentNullException.ThrowIfNull(outboxRecord);

        var outboxCollection = GetOutboxCollection();
        await outboxCollection.InsertOneAsync(outboxRecord);
    }

    private IMongoCollection<OutboxRecord> GetOutboxCollection()
    {
        var database = _mongoClient.GetDatabase(_outboxSettings.DatabaseName);
        return database.GetCollection<OutboxRecord>(CollectionName);
    }

    private static Task PersistIndividually(IMongoCollection<OutboxRecord> outboxCollection, IReadOnlyCollection<OutboxRecord> outboxRecords)
    {
        var writes = new List<InsertOneModel<OutboxRecord>>(outboxRecords.Count);
        foreach (var outboxRecord in outboxRecords)
        {
            var writeModel = new InsertOneModel<OutboxRecord>(outboxRecord);
            writes.Add(writeModel);
        }
        return outboxCollection.BulkWriteAsync(writes);
    }
}