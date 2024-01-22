using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;

internal sealed class MongoDbTransactionProvider : ITransactionProvider
{
    private readonly MongoClient _mongoClient;

    private IOutboxTransaction? _currentTransaction;

    public MongoDbTransactionProvider(MongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public async Task<IOutboxTransaction> NewTransaction(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("A transaction has already been started.");

        var session = await _mongoClient.StartSessionAsync(null, cancellationToken);
        _currentTransaction = new MongoDbOutboxTransaction(session);
        await _currentTransaction.StartTransaction(cancellationToken);
        return _currentTransaction;
    }

    public bool TryGetCurrentTransaction(out IOutboxTransaction? outboxTransaction)
    {
        outboxTransaction = _currentTransaction;
        return _currentTransaction is not null;
    }
}