﻿using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, MongoClient mongoClient)
    {
        builder.Services.AddScoped<IPersistDomainEvents>(sp => ActivatorUtilities.CreateInstance<MongoDbDomainEventPersister>(sp, builder.OutboxSettings, mongoClient));
        return builder;
    }

    public static IOutboxDispatcherBuilder AddMongo(this IOutboxDispatcherBuilder builder, Func<IServiceProvider, MongoClient> mongoClientFactory)
    {
        builder.Services.AddScoped<IPersistDomainEvents>(sp => ActivatorUtilities.CreateInstance<MongoDbDomainEventPersister>(sp, builder.OutboxSettings, mongoClientFactory(sp)));
        return builder;
    }
}
