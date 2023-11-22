namespace MinimalDomainEvents.Dispatcher;

public interface IDomainEventDispatcher
{
    Task DispatchAndClear();
}
