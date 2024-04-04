using MongoTestContainer;

namespace MinimalDomainEvents.Outbox.Worker.UnitTests;
[CollectionDefinition("MongoDb Integration")]
public sealed class MongoContainerCollectionDefinition : ICollectionFixture<MongoContainerFixture> { }