namespace Nevsnirg.DomainEvents.Dispatcher;

public interface IDomainEventDispatcher
{
    Task DispatchAndClear();
}
