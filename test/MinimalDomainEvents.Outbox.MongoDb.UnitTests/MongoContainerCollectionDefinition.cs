using MongoTestContainer;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;
[CollectionDefinition("MongoDb Integration")]
public partial class MongoContainerCollectionDefinition : ICollectionFixture<MongoContainerFixture> { }
