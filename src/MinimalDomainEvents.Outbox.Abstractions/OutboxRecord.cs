namespace MinimalDomainEvents.Outbox.Abstractions;
public sealed class OutboxRecord
{
    public DateTimeOffset EnqueuedAt { get; init; }
    public object MessageData { get; init; }

    public OutboxRecord(DateTimeOffset enqueuedAt, object messageData)
    {
        EnqueuedAt = enqueuedAt;
        MessageData = messageData;
    }
}