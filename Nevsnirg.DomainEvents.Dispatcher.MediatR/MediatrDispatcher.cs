using MediatR;
using Nevsnirg.DomainEvents.Contract;

namespace Nevsnirg.DomainEvents.Dispatcher.MediatR;

public class MediatrDispatcher : ScopedDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public MediatrDispatcher(IMediator mediator) : base()
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
