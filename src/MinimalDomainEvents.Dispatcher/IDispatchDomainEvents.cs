using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Dispatcher;
public interface IDispatchDomainEvents
{
    Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents);
}
