using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher.MediatR;

public static class IDomainEventDispatcherBuilderExtensions
{
    public static IServiceCollection AddMediatorDispatcher(this IDomainEventDispatcherBuilder builder)
    {
        return builder.Services
            .AddScoped<IDispatchDomainEvents, MediatorDispatcher>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>))
            ;
    }
}
