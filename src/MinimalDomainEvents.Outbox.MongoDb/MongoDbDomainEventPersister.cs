using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

//TODO - Transactions aan/uit zetten
namespace MinimalDomainEvents.Outbox.MongoDb;
internal sealed class MongoDbDomainEventPersister : IPersistDomainEvents
{
    private readonly OutboxSettings _outboxSettings;
    private readonly MongoClient _mongoClient;

    public MongoDbDomainEventPersister(OutboxSettings outboxSettings, MongoClient mongoClient)
    {
        _outboxSettings = outboxSettings;
        _mongoClient = mongoClient;
    }

    public async Task Persist(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        var database = _mongoClient.GetDatabase(_outboxSettings.DatabaseName);
        var outboxCollection = database.GetCollection<OutboxRecord>("OutboxRecords");

        if(_outboxSettings.SendBatched)
            await PersistBatched(outboxCollection, domainEvents);
        else
            await PersistIndividually(outboxCollection, domainEvents);
    }

    private static Task PersistBatched(IMongoCollection<OutboxRecord> outboxCollection, IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        var outboxRecord = new OutboxRecord(DateTimeOffset.UtcNow, domainEvents);
        return outboxCollection.InsertOneAsync(outboxRecord);
    }

    private static Task PersistIndividually(IMongoCollection<OutboxRecord> outboxCollection, IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        var enqueuedAt = DateTimeOffset.UtcNow;
        var writes = new List<InsertOneModel<OutboxRecord>>(domainEvents.Count);
        foreach (var domainEvent in domainEvents)
        {
            var outboxRecord = new OutboxRecord(enqueuedAt, domainEvent);
            var writeModel = new InsertOneModel<OutboxRecord>(outboxRecord);
            writes.Add(writeModel);
        }
        return outboxCollection.BulkWriteAsync(writes);
    }
}