using Microsoft.AspNetCore.Http;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher.AspNetCore;
internal sealed class DomainEventDispatcherMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next, IScopedDomainEventDispatcher domainEventDispatcher)
    {
        await next(context);
        await domainEventDispatcher.DispatchAndClear();
    }
}
