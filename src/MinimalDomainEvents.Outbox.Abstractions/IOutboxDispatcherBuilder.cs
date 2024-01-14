using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IOutboxDispatcherBuilder
{
    IServiceCollection Services { get; }
    OutboxSettings OutboxSettings { get; }
}

internal sealed record OutboxDispatcherBuilder(IServiceCollection Services) : IOutboxDispatcherBuilder
{
    public OutboxSettings OutboxSettings { get; } = new();
}