using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox;

internal sealed record OutboxDispatcherBuilder(IServiceCollection Services) : IOutboxDispatcherBuilder
{
    public OutboxSettings OutboxSettings { get; } = new();
}