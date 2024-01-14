using Microsoft.AspNetCore.Builder;

namespace MinimalDomainEvents.Dispatcher.AspNetCore;
public static class DependencyInjectionConfiguration
{
    public static IApplicationBuilder UseDomainEventDispatcherMiddleware(this IApplicationBuilder applicationBuilder) => applicationBuilder.UseMiddleware<DomainEventDispatcherMiddleware>();
}