using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Nevsnirg.DomainEvents.Dispatcher.MediatR.MicrosoftDependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMediatrDispatcher(this IServiceCollection services)
    {
        return services
            .AddScoped<IDomainEventDispatcher, MediatorDispatcher>()
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>))
            ;
    }
}
