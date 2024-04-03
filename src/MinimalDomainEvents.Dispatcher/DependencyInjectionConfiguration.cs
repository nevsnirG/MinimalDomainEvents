using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher;
public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        return AddDomainEventDispatcher(services, null, serviceLifetime);
    }

    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services, Action<IDomainEventDispatcherBuilder>? configure, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        services.Add(new ServiceDescriptor(typeof(IScopedDomainEventDispatcher), typeof(ScopedDomainEventDispatcher), serviceLifetime));

        if (configure is not null)
        {
            var builder = new DomainEventDispatcherBuilder(services);
            configure(builder);
        }

        return services;
    }
}
