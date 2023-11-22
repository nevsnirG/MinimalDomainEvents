# Nevsnirg.DomainEvents
Minimal impact domain event raising and dispatching framework

## Nevsnirg.DomainEvents.Contract
Contains the IDomainEvent marker interface.

## Nevsnirg.DomainEvents.Core
Reference this project from your Domain project. Exposes the DomainEventTracker class for raising domain events from your domain entities, and the IDomainEventScope interface for scoping the registration of domain events.

## Nevsnirg.DomainEvents.Dispatcher
Holds the abstract ScopedDomainEventDispatcher class which creates a scope on construction, used for scoping the registration of domain events to the lifetime of this class.

## Nevsnirg.DomainEvents.Dispatcher.MediatR
Contains the MediatorDispatcher, which dispatches the domain events using MediatR, as well as the DomainEventDispatchBehavior. The behavior uses the lifetime of the MediatorDispatcher to capture registered domain events during its lifetime and dispatches them when the RequestHandlerDelegate completes successfully.

## Nevsnirg.DomainEvents.Dispatcher.MediatR.MicrosoftDependencyInjection
IServiceCollection extension method for registering the MediatorDispatcher and DOmainEventDispatchBehavior.
