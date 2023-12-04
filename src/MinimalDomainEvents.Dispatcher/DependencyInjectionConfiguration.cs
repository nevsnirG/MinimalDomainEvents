using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher;
public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        return AddDomainEventDispatcher(services, null);
    }

    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services, Action<IDomainEventDispatcherBuilder>? configure)
    {
        services.AddScoped<IDomainEventDispatcher, ScopedDomainEventDispatcher>();

        if (configure is not null)
        {
            var builder = new DomainEventDispatcherBuilder(services);
            configure(builder);
        }

        return services;
    }
}
