using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Dispatcher.Abstractions;
public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        return services.AddDomainEventDispatcher(null, serviceLifetime);
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
