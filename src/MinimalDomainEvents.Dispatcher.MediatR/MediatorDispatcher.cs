using MediatR;
using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Dispatcher.MediatR;

internal sealed class MediatorDispatcher : ScopedDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public MediatorDispatcher(IMediator mediator) : base()
    {
        _mediator = mediator;
    }

    protected override async Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}
