using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Dispatcher.Abstractions;

public interface IDomainEventDispatcherBuilder
{
    IServiceCollection Services { get; }
}
