namespace MinimalDomainEvents.Outbox.Abstractions;

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