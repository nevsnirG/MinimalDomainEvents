# MinimalDomainEvents
Minimal impact domain event raising and dispatching framework leveraging AsyncLocal to scope and track domain events raised from your domain model across the asynchronous execution context. This allows for the tracking of domain events raised in an entity, without the need for exposing a public collection of domain events on the entity, which needs to be (manually) tracked. Domain events are always raised in the current active scope. The current active scope is the deepest scope active where the entity resides in.

## Packages
### MinimalDomainEvents.Contract
Contains the IDomainEvent marker interface. Make sure your domain events implement this interface.

### MinimalDomainEvents.Core
Reference this project from your Domain project. Exposes the DomainEventTracker class for raising domain events from your domain entities, and the IDomainEventScope interface for scoping the raising of domain events

#### Examples
Raising a domain event from inside your entity.
```csharp
public class Customer : IEntity
{
    public Order PlaceOrder(string productId)
    {
        DomainEventTracker.RaiseDomainEvent(new OrderPlacedDomainEvent { ProductId = productId });
        return new Order(productId);
    }
}
```
Raising a domain event on a specific scope.
```csharp
using (var scope = DomainEventTracker.CreateScope())
{
    scope.RaiseDomainEvent(new TestEvent());
}
```
Domain events raised using the static DomainEventTracker are raised on the current active scope (the deepest existing scope). Calling Peek on the DomainEventTracker yields the same result als calling Peek on the current active scope.
```csharp
using (var scope = DomainEventTracker.CreateScope())
{
    DomainEventTracker.RaiseDomainEvent(new TestEvent("I was raised in the top scope."));
    using (var nestedScope = DomainEventTracker.CreateScope())
    {
        DomainEventTracker.RaiseDomainEvent(new TestEvent("I was raised in the nested scope."));

        using (var evenMoreNestedScope = DomainEventTracker.CreateScope())
        {
            DomainEventTracker.RaiseDomainEvent(new TestEvent("I was raised in the deepest scope."));
            DomainEventTracker.Peek().Should().HaveCount(1);
        }

        DomainEventTracker.RaiseDomainEvent(new TestEvent("I was also raised in the nested scope."));
        DomainEventTracker.Peek().Should().HaveCount(2);
    }

    DomainEventTracker.Peek().Should().HaveCount(1);
}

DomainEventTracker.Peek().Should().BeEmpty();
```


### MinimalDomainEvents.Dispatcher
Holds the ScopedDomainEventDispatcher class which creates a scope on construction, used for scoping the raising of domain events to the lifetime of this class. Resolves all registered IDispatchDomainEvents interface implementations for dispatching the raised domain events. Dispatches all domain events raised in its own scope.

### MinimalDomainEvents.Dispatcher.MediatR
Contains the MediatorDispatcher, which dispatches the domain events using MediatR. The behavior uses the lifetime of the ScopedDomainEventDispatcher to capture raised domain events during its lifetime and dispatches them when the RequestHandlerDelegate completes successfully. Make sure your domain events implement both IDomainEvent and INotification.

#### Usage (Microsoft.Extensions.DependencyInjection)
```csharp
using MinimalDomainEvents.Dispatcher.MediatR;

IServiceCollection services = new ServiceCollection();
services.AddDomainEventDispatcher(b => b.AddMediatorDispatcher());
```

### MinimalDomainEvents.Dispatcher.AspNetCore
Registers a conventional middleware to track domain events across a request processing pipeline.

```csharp
app.UseDomainEventDispatcherMiddleware();
```