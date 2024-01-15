using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IOutboxDispatcherBuilder
{
    IServiceCollection Services { get; }
    OutboxSettings OutboxSettings { get; }
}
