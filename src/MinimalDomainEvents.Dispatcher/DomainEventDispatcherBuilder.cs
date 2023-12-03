using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Dispatcher;

public interface IDomainEventDispatcherBuilder
{
    IServiceCollection Services { get; }
}

internal sealed record DomainEventDispatcherBuilder(IServiceCollection Services) : IDomainEventDispatcherBuilder;