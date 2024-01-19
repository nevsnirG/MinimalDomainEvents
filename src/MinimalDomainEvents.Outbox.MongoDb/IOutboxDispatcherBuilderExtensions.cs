using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, MongoClient mongoClient)
    {
        RegisterDefaultServices(builder);

        builder.Services.AddScoped<IPersistOutboxRecords>(sp => ActivatorUtilities.CreateInstance<MongoDbDomainEventPersister>(sp, builder.OutboxSettings, mongoClient));
        return builder;
    }

    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, Func<IServiceProvider, MongoClient> mongoClientFactory)
    {
        RegisterDefaultServices(builder);

        builder.Services.AddScoped<IPersistOutboxRecords>(sp => ActivatorUtilities.CreateInstance<MongoDbDomainEventPersister>(sp, builder.OutboxSettings, mongoClientFactory(sp)));
        return builder;
    }

    private static void RegisterDefaultServices(IOutboxDispatcherBuilder builder)
    {
        TryRegisterOutboxRecordClassMap();

        builder.Services.TryAddScoped<MongoSessionProvider>();
        builder.Services.TryAddScoped<IMongoSessionProvider>(sp => sp.GetRequiredService<MongoSessionProvider>());
        builder.Services.TryAddScoped<IMongoSessionProviderInitializer>(sp => sp.GetRequiredService<MongoSessionProvider>());
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
