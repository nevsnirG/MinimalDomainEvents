using MediatR;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher.MediatR;

internal sealed class DomainEventDispatchBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IScopedDomainEventDispatcher _domainEventDispatcher;

    public DomainEventDispatchBehavior(IScopedDomainEventDispatcher domainEventDispatcher)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        await _domainEventDispatcher.DispatchAndClear();

        return response;
    }
}
