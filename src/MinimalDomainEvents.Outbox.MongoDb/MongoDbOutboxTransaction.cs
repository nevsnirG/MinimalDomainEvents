using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;

internal sealed class MongoDbOutboxTransaction : IOutboxTransaction
{
    public IClientSessionHandle ClientSessionHandle { get; }

    private Action? _onCommit;

    public MongoDbOutboxTransaction(IClientSessionHandle clientSessionHandle)
    {
        ClientSessionHandle = clientSessionHandle;
    }

    public Task StartTransaction(Action? onCommit = null, CancellationToken cancellationToken = default)
    {
        ClientSessionHandle.StartTransaction();
        _onCommit = onCommit;
        return Task.CompletedTask;
    }

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        await ClientSessionHandle.CommitTransactionAsync(cancellationToken);
        _onCommit?.Invoke();
    }

    public void Dispose()
    {
        ClientSessionHandle.Dispose();
    }
}