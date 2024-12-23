﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, MongoClient mongoClient)
    {
        RegisterDefaultServices(builder);

        builder.Services.TryAddSingleton(mongoClient);
        return builder;
    }

    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, Func<IServiceProvider, MongoClient> mongoClientFactory)
    {
        RegisterDefaultServices(builder);

        builder.Services.TryAddSingleton(sp => mongoClientFactory(sp));
        return builder;
    }

    private static void RegisterDefaultServices(IOutboxDispatcherBuilder builder)
    {
        builder.Services.TryAddScoped<IPersistOutboxRecords, MongoDbOutboxRecordPersister>();
        builder.Services.TryAddScoped<MongoSessionProvider>();
        builder.Services.TryAddScoped<IMongoSessionProvider>(sp => sp.GetRequiredService<MongoSessionProvider>());
        builder.Services.TryAddScoped<IMongoSessionProviderInitializer>(sp => sp.GetRequiredService<MongoSessionProvider>());
        builder.Services.TryAddScoped<OutboxRecordCollectionProvider>();
        builder.Services.TryAddScoped<IOutboxRecordCollectionProvider>(sp => sp.GetRequiredService<OutboxRecordCollectionProvider>());
        builder.Services.TryAddScoped<IOutboxRecordCollectionInitializer>(sp => sp.GetRequiredService<OutboxRecordCollectionProvider>());
        builder.Services.TryAddScoped<ITransactionProvider, MongoDbTransactionProvider>();
        builder.Services.TryAddScoped<IRetrieveOutboxRecords, MongoDbOutboxRecordRetriever>();
    }
}
