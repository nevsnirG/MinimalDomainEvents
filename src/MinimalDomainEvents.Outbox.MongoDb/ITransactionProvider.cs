using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
public interface ITransactionProvider
{
    IClientSessionHandle? Session { get; }
}

public interface ITransactionProviderInitializer
{
    void Initialize(IClientSessionHandle session);
}

internal sealed class TransactionProvider : ITransactionProvider, ITransactionProviderInitializer
{
    public IClientSessionHandle? Session => _session;

    private IClientSessionHandle? _session;

    public void Initialize(IClientSessionHandle session)
    {
        _session = session;
    }
}