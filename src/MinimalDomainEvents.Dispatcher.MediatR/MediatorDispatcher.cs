using MediatR;
using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Dispatcher.MediatR;

public class MediatorDispatcher : ScopedDomainEventDispatcher
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
