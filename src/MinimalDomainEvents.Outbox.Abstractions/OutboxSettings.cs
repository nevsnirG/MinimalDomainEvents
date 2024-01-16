namespace MinimalDomainEvents.Outbox.Abstractions;

public sealed class OutboxSettings
{
    public static OutboxSettings Default => new()
    {
        SendBatched = true
    };

    public string? DatabaseName { get; set; }
    public bool SendBatched { get; set; }
}