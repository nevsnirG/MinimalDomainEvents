using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
internal interface IOutboxRecordCollectionProvider
{
    IMongoCollection<OutboxRecord> Provide(MongoCollectionSettings? collectionSettings = null);
}

internal sealed class OutboxRecordCollectionProvider : IOutboxRecordCollectionProvider, IOutboxRecordCollectionInitializer
{
    private const string CollectionName = "OutboxRecords";

    private readonly OutboxSettings _outboxSettings;
    private readonly MongoClient _mongoClient;

    public OutboxRecordCollectionProvider(OutboxSettings outboxSettings, MongoClient mongoClient)
    {
        _outboxSettings = outboxSettings;
        _mongoClient = mongoClient;
    }

    //TODO - Indexes op EnqueuedAt en DispatchedAt
    //TODO - Expiration indexes
    public Task Initialize(CancellationToken cancellationToken)
    {
        TryRegisterOutboxRecordClassMap();

        var collectionSettings = new MongoCollectionSettings
        {
            ReadConcern = ReadConcern.Majority,
            ReadPreference = ReadPreference.Primary,
            WriteConcern = WriteConcern.WMajority
        };

        var collection = Provide(collectionSettings);

        //TODO - Create indexes (recreate if exists with same name (because of versioning))
        return Task.CompletedTask;
    }

    private static bool TryRegisterOutboxRecordClassMap()
    {
        return BsonClassMap.TryRegisterClassMap<OutboxRecord>(cm =>
        {
            cm.MapIdProperty(or => or.Id);
            cm.SetIgnoreExtraElements(true);
        });
    }

    public IMongoCollection<OutboxRecord> Provide(MongoCollectionSettings? collectionSettings = null)
    {
        var database = _mongoClient.GetDatabase(_outboxSettings.DatabaseName);
        return database.GetCollection<OutboxRecord>(CollectionName, collectionSettings);
    }
}
