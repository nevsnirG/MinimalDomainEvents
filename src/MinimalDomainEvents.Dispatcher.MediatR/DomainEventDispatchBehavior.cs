using MediatR;
using MinimalDomainEvents.Core;

namespace MinimalDomainEvents.Dispatcher.MediatR;

internal sealed class DomainEventDispatchBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public DomainEventDispatchBehavior(IDomainEventDispatcher domainEventDispatcher)
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
