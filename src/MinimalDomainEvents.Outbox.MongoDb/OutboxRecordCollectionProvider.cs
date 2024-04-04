using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Bson;
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

    private const string EnqueuedAtIndexName = "EnqueuedAt_asc";
    private const string DispatchedAtIndexName = "OutboxCleanup";

    public OutboxRecordCollectionProvider(OutboxSettings outboxSettings, MongoClient mongoClient)
    {
        _outboxSettings = outboxSettings;
        _mongoClient = mongoClient;
    }

    public async Task Initialize(CancellationToken cancellationToken)
    {
        TryRegisterOutboxRecordClassMap();

        var collectionSettings = new MongoCollectionSettings
        {
            ReadConcern = ReadConcern.Majority,
            ReadPreference = ReadPreference.Primary,
            WriteConcern = WriteConcern.WMajority
        };

        var collection = Provide(collectionSettings);
        var existingIndexes = await GetExistingIndexes(collection, cancellationToken);

        //TODO - Recreate indexes if changed.

        if (CanCreateEnqueuedAtIndex(existingIndexes))
            await CreateEnqueuedAtIndex(collection, cancellationToken);

        if (CanCreateDispatchedAtIndex(existingIndexes))
            await CreateDispatchedAtIndex(collection, cancellationToken);
    }

    private static async Task<List<BsonDocument>> GetExistingIndexes(IMongoCollection<OutboxRecord> collection, CancellationToken cancellationToken)
    {
        var existingIndexesCursor = await collection.Indexes.ListAsync(cancellationToken);
        return await existingIndexesCursor.ToListAsync(cancellationToken);
    }

    private static bool CanCreateEnqueuedAtIndex(List<BsonDocument> existingIndexes)
    {
        return !existingIndexes.Any(i => i["name"].AsString == EnqueuedAtIndexName);
    }

    private static bool CanCreateDispatchedAtIndex(List<BsonDocument> existingIndexes)
    {
        return !existingIndexes.Any(i => i["name"].AsString == DispatchedAtIndexName);
    }

    private static async Task CreateEnqueuedAtIndex(IMongoCollection<OutboxRecord> collection, CancellationToken cancellationToken)
    {
        var indexKeysDefinition = Builders<OutboxRecord>.IndexKeys.Ascending(or => or.EnqueuedAt);
        var createIndexModel = new CreateIndexModel<OutboxRecord>(indexKeysDefinition, new()
        {
            Name = EnqueuedAtIndexName,
            Background = true
        });
        await collection.Indexes.CreateOneAsync(createIndexModel, null, cancellationToken);
    }

    private static async Task CreateDispatchedAtIndex(IMongoCollection<OutboxRecord> collection, CancellationToken cancellationToken)
    {
        var indexKeysDefinition = Builders<OutboxRecord>.IndexKeys.Ascending(or => or.DispatchedAt);
        var createIndexModel = new CreateIndexModel<OutboxRecord>(indexKeysDefinition, new()
        {
            ExpireAfter = TimeSpan.FromDays(7),
            Background = true,
            Name = DispatchedAtIndexName
        });
        await collection.Indexes.CreateOneAsync(createIndexModel, null, cancellationToken);
    }

    private static bool TryRegisterOutboxRecordClassMap()
    {
        return BsonClassMap.TryRegisterClassMap<OutboxRecord>(cm =>
        {
            cm.MapIdProperty(or => or.Id);
            cm.AutoMap();
            cm.UnmapProperty(or => or.ExpiresAt);
            cm.SetIgnoreExtraElements(true);
        });
    }

    public IMongoCollection<OutboxRecord> Provide(MongoCollectionSettings? collectionSettings = null)
    {
        var database = _mongoClient.GetDatabase(_outboxSettings.DatabaseName);
        return database.GetCollection<OutboxRecord>(CollectionName, collectionSettings);
    }
}
