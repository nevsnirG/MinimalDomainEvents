using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, MongoClient mongoClient)
    {
        RegisterDefaultServices(builder);

        builder.Services.AddScoped<IPersistOutboxRecords>(sp => ActivatorUtilities.CreateInstance<MongoDbOutboxRecordPersister>(sp, builder.OutboxSettings, mongoClient));
        return builder;
    }

    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, Func<IServiceProvider, MongoClient> mongoClientFactory)
    {
        RegisterDefaultServices(builder);

        builder.Services.AddScoped<IPersistOutboxRecords>(sp => ActivatorUtilities.CreateInstance<MongoDbOutboxRecordPersister>(sp, builder.OutboxSettings, mongoClientFactory(sp)));
        return builder;
    }

    private static void RegisterDefaultServices(IOutboxDispatcherBuilder builder)
    {
        builder.Services.TryAddScoped<MongoSessionProvider>();
        builder.Services.TryAddScoped<IMongoSessionProvider>(sp => sp.GetRequiredService<MongoSessionProvider>());
        builder.Services.TryAddScoped<IMongoSessionProviderInitializer>(sp => sp.GetRequiredService<MongoSessionProvider>());
        builder.Services.TryAddScoped<IRetrieveOutboxRecords, MongoDbOutboxRecordRetriever>();
        builder.Services.TryAddScoped<IOutboxRecordCollectionProvider, OutboxRecordCollectionProvider>();
        builder.Services.TryAddScoped<ITransactionProvider, MongoTransactionProvider>();
    }
}
