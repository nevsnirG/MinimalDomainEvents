using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox;

public static class IDomainEventDispatcherBuilderExtensions
{
    public static IDomainEventDispatcherBuilder AddOutbox(this IDomainEventDispatcherBuilder builder)
    {
        builder.Services.RemoveAll<IScopedDomainEventDispatcher>();
        builder.Services
            .AddScoped<IScopedDomainEventDispatcher, OutboxDomainEventDispatcher>()
            ;

        return builder;
    }

    public static IDomainEventDispatcherBuilder AddOutbox(this IDomainEventDispatcherBuilder builder, Action<IOutboxDispatcherBuilder>? configure)
    {
        AddOutbox(builder);

        if (configure is not null)
        {
            var outboxDispatcherBuilder = new OutboxDispatcherBuilder(builder.Services);
            builder.Services.AddSingleton(outboxDispatcherBuilder.OutboxSettings);
            configure(outboxDispatcherBuilder);
        }

        return builder;
    }
}