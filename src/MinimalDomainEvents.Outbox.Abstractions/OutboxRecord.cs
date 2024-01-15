namespace MinimalDomainEvents.Outbox.Abstractions;
//TODO - ExpirateAt property
public sealed class OutboxRecord
{
    public DateTimeOffset EnqueuedAt { get; init; }
    public byte[] MessageData { get; init; }
    public DateTimeOffset ExpiresAt => EnqueuedAt.AddDays(7);

    public OutboxRecord(DateTimeOffset enqueuedAt, byte[] messageData)
    {
        EnqueuedAt = enqueuedAt;
        MessageData = messageData;
    }
}