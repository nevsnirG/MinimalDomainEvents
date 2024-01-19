using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb;
public interface IMongoSessionProvider
{
    IClientSessionHandle? Session { get; }
}

public interface IMongoSessionProviderInitializer
{
    void Initialize(IClientSessionHandle session);
}

internal sealed class MongoSessionProvider : IMongoSessionProvider, IMongoSessionProviderInitializer
{
    public IClientSessionHandle? Session => _session;

    private IClientSessionHandle? _session;

    public void Initialize(IClientSessionHandle session)
    {
        _session = session;
    }
}