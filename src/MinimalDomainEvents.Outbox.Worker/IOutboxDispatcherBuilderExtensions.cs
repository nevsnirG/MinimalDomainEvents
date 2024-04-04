using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker;

public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder WithHostingDispatcher(this IOutboxDispatcherBuilder builder)
    {
        builder.Services.TryAddScoped<IDomainEventRetriever, DomainEventRetriever>();
        builder.Services.AddHostedService<BackgroundDispatchWorker>();
        return builder;
    }
}