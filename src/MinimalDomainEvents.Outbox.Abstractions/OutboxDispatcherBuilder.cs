using Microsoft.Extensions.DependencyInjection;

namespace MinimalDomainEvents.Outbox.Abstractions;

internal sealed record OutboxDispatcherBuilder(IServiceCollection Services) : IOutboxDispatcherBuilder
{
    public OutboxSettings OutboxSettings { get; } = OutboxSettings.Default;
}