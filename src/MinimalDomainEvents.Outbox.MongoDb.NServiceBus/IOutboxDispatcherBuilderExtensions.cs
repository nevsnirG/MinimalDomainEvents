using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.MongoDb.NServiceBus;
public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder WithNServiceBusMongoStorageProvider(this IOutboxDispatcherBuilder builder)
    {
        builder.Services.RemoveAll<IMongoSessionProvider>();
        builder.Services.AddScoped<IMongoSessionProvider, NServiceBusStorageSessionProvider>();
        return builder;
    }
}