using MediatR;
using Nevsnirg.DomainEvents.Core;

namespace Nevsnirg.DomainEvents.Dispatcher.MediatR;

public class DomainEventDispatchBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public DomainEventDispatchBehavior(IDomainEventDispatcher domainEventDispatcher)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var scope = DomainEventTracker.CreateScope();

        var response = await next();

        await _domainEventDispatcher.DispatchAndClear();

        return response;
    }
}
