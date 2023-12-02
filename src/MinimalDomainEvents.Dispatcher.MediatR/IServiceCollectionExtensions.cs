using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Dispatcher.MediatR;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMediatorDispatcher(this IServiceCollection services)
    {
        return services
            .AddScoped<IDomainEventDispatcher, MediatorDispatcher>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>))
            ;
    }
}
