using MediatR;
using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Dispatcher.MediatR;

internal sealed class MediatorDispatcher : IDispatchDomainEvents
{
    private readonly IMediator _mediator;

    public MediatorDispatcher(IMediator mediator) : base()
    {
        _mediator = mediator;
    }

    public async Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}
