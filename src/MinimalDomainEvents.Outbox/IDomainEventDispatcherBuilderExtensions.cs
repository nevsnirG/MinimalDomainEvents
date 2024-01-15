using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox;

public static class IDomainEventDispatcherBuilderExtensions
{
    public static IDomainEventDispatcherBuilder AddOutbox(this IDomainEventDispatcherBuilder builder)
    {
        builder.Services
            .AddScoped<IDispatchDomainEvents, OutboxDomainEventDispatcher>()
            ;

        return builder;
    }

    public static IDomainEventDispatcherBuilder AddOutbox(this IDomainEventDispatcherBuilder builder, Action<IOutboxDispatcherBuilder>? configure)
    {
        builder.Services
            .AddScoped<IDispatchDomainEvents, OutboxDomainEventDispatcher>()
            ;

        if (configure is not null)
        {
            configure(new OutboxDispatcherBuilder(builder.Services));
        }

        return builder;
    }
}