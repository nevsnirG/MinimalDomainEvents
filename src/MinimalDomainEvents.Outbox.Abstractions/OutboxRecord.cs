namespace MinimalDomainEvents.Outbox.Abstractions;
public sealed class OutboxRecord
{
    public Guid Id { get; set; }
    public DateTimeOffset EnqueuedAt { get; init; }
    public DateTimeOffset? DispatchedAt { get; set; }
    public byte[] MessageData { get; set; }
    public DateTimeOffset ExpiresAt => EnqueuedAt.AddDays(7);

    public OutboxRecord(DateTimeOffset enqueuedAt, byte[] messageData)
    {
        EnqueuedAt = enqueuedAt;
        MessageData = messageData;
    }
}