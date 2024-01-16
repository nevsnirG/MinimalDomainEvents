using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;
public class TransactionProviderTests
{
    [Fact]
    internal void Given_Initialized_Returns_Session()
    {
        var session = Mock.Of<IClientSessionHandle>();
        var sut = new TransactionProvider();

        sut.Initialize(session);

        sut.Session.Should().BeSameAs(session, "the transaction provider should return the session it was initialized with.");
    }
}