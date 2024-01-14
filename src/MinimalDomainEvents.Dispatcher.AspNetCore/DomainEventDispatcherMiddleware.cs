using Microsoft.AspNetCore.Http;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher.AspNetCore;
internal sealed class DomainEventDispatcherMiddleware : IMiddleware
{
    private readonly IScopedDomainEventDispatcher _domainEventDispatcher;

    public DomainEventDispatcherMiddleware(IScopedDomainEventDispatcher domainEventDispatcher)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);
        await _domainEventDispatcher.DispatchAndClear();
    }
}
