using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Dispatcher.Abstractions;

internal sealed record DomainEventDispatcherBuilder(IServiceCollection Services) : IDomainEventDispatcherBuilder;