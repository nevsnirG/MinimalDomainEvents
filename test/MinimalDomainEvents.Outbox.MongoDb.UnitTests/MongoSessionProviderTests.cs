using MongoDB.Driver;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;
public class MongoSessionProviderTests
{
    [Fact]
    internal void Given_Initialized_Returns_Session()
    {
        var session = Mock.Of<IClientSessionHandle>();
        var sut = new MongoSessionProvider();

        sut.Initialize(session);

        sut.Session.Should().BeSameAs(session, "the session provider should return the session it was initialized with.");
    }
}