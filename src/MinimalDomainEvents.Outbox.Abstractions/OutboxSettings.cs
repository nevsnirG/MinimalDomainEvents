namespace MinimalDomainEvents.Outbox.Abstractions;

public sealed class OutboxSettings
{
    public string? DatabaseName { get; set; }
    public bool SendBatched { get; set; } = true;
}