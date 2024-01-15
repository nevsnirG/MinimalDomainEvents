using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, MongoClient mongoClient)
    {
        TryRegisterOutboxRecordClassMap();
        builder.Services.AddScoped<IPersistOutboxRecords>(sp => ActivatorUtilities.CreateInstance<MongoDbDomainEventPersister>(sp, builder.OutboxSettings, mongoClient));
        return builder;
    }

    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, Func<IServiceProvider, MongoClient> mongoClientFactory)
    {
        TryRegisterOutboxRecordClassMap();
        builder.Services.AddScoped<IPersistOutboxRecords>(sp => ActivatorUtilities.CreateInstance<MongoDbDomainEventPersister>(sp, builder.OutboxSettings, mongoClientFactory(sp)));
        return builder;
    }

    private static bool TryRegisterOutboxRecordClassMap()
    {
        return BsonClassMap.TryRegisterClassMap<OutboxRecord>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        });
    }
}
