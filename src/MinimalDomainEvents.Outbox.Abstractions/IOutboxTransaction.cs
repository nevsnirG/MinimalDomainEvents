namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IOutboxTransaction : IDisposable
{
    Task StartTransaction(Action? onCommit = null, CancellationToken cancellationToken = default);
    Task Commit(CancellationToken cancellationToken = default);
}