using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Dispatcher.Abstractions;
public interface IDispatchDomainEvents
{
    Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents);
}
