using MongoTestContainer;

namespace MinimalDomainEvents.Outbox.Worker.UnitTests;
[CollectionDefinition("MongoDb Integration")]
public partial class MongoContainerCollectionDefinition : ICollectionFixture<MongoContainerFixture> { }