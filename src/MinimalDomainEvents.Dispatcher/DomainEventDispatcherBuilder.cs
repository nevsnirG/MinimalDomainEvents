using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher;

internal sealed record DomainEventDispatcherBuilder(IServiceCollection Services) : IDomainEventDispatcherBuilder;