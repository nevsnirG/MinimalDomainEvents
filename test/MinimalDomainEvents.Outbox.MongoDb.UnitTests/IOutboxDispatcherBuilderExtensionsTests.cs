using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;

[CollectionDefinition("MongoDb Integration")]
public class IOutboxDispatcherBuilderExtensionsTests(MongoContainerFixture fixture)
{
    [Fact]
    public async Task WithDatabase_AfterUseMongo_StillWorks()
    {
        var mongoClient = new MongoClient(fixture.ConnectionString);
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddDomainEventDispatcher(dispatcherBuilder =>
        {
            dispatcherBuilder.AddOutbox(outboxBuilder =>
            {
                outboxBuilder.AddMongo(mongoClient);
                outboxBuilder.WithDatabase("testdatabase");
            });
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var persister = serviceProvider.GetRequiredService<IPersistDomainEvents>();

        await persister.Persist(new List<IDomainEvent>(1)
        {
            new TestEvent()
        });

        var outboxCollection = mongoClient.GetDatabase("testdatabase").GetCollection<OutboxRecord>("OutboxRecords");
        var item = await outboxCollection.AsQueryable().SingleOrDefaultAsync();
        item.Should().NotBeNull();
    }

    public sealed record TestEvent() : IDomainEvent;
}