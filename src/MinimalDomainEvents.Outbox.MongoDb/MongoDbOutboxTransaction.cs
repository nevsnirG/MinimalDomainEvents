using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;

internal sealed class MongoDbOutboxTransaction : IOutboxTransaction
{
    public IClientSessionHandle ClientSessionHandle { get; }

    public MongoDbOutboxTransaction(IClientSessionHandle clientSessionHandle)
    {
        ClientSessionHandle = clientSessionHandle;
    }

    public Task StartTransaction(CancellationToken cancellationToken = default)
    {
        ClientSessionHandle.StartTransaction();
        return Task.CompletedTask;
    }

    public Task Commit(CancellationToken cancellationToken = default)
    {
        return ClientSessionHandle.CommitTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        ClientSessionHandle.Dispose();
    }
}