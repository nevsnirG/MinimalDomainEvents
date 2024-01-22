namespace MinimalDomainEvents.Outbox.Abstractions;
public interface IOutboxTransaction : IDisposable
{
    Task StartTransaction(CancellationToken cancellationToken = default);
    Task Commit(CancellationToken cancellationToken = default);
}