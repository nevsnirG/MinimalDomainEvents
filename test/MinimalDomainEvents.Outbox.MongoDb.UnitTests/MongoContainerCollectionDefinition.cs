using MongoTestContainer;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;
[CollectionDefinition("MongoDb Integration")]
public sealed class MongoContainerCollectionDefinition : ICollectionFixture<MongoContainerFixture> { }