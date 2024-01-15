using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox;

public static class IOutboxDispatcherBuilderExtensions
{
    public static IOutboxDispatcherBuilder WithDatabase(this IOutboxDispatcherBuilder builder, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentNullException(nameof(databaseName));

        builder.OutboxSettings.DatabaseName = databaseName;

        return builder;
    }
}