using MongoDB.Driver;
using NServiceBus.Storage.MongoDB;

namespace MinimalDomainEvents.Outbox.MongoDb.NServiceBus;
internal sealed class NServiceBusStorageSessionProvider : ITransactionProvider
{
    private readonly IMongoSynchronizedStorageSession _sharedSession;

    public NServiceBusStorageSessionProvider(IMongoSynchronizedStorageSession sharedSession)
    {
        _sharedSession = sharedSession;
    }

    public IClientSessionHandle? Session => _sharedSession.MongoSession;
}