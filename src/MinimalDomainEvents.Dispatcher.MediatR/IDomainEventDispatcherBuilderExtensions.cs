using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher.MediatR;

public static class IDomainEventDispatcherBuilderExtensions
{
    public static IDomainEventDispatcherBuilder AddMediatorDispatcher(this IDomainEventDispatcherBuilder builder)
    {
        builder.Services
            .AddScoped<IDispatchDomainEvents, MediatorDispatcher>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>))
            ;

        return builder;
    }
}
