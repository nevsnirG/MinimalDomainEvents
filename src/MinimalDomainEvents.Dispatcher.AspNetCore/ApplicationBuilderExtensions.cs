using Microsoft.AspNetCore.Builder;

namespace MinimalDomainEvents.Dispatcher.AspNetCore;
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseDomainEventDispatcherMiddleware(this IApplicationBuilder applicationBuilder)
    {
        return applicationBuilder.UseMiddleware<DomainEventDispatcherMiddleware>();
    }
}