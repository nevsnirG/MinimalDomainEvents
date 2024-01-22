namespace MinimalDomainEvents.Outbox.Abstractions;

public interface ITransactionProvider
{
    Task<IOutboxTransaction> NewTransaction(CancellationToken cancellationToken = default);
    bool TryGetCurrentTransaction(out IOutboxTransaction? outboxTransaction);
}