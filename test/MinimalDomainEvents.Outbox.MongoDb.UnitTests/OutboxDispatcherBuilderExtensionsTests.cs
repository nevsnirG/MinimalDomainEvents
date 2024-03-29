using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;

[Collection("MongoDb Integration")]
public class OutboxDispatcherBuilderExtensionsTests(MongoContainerFixture fixture)
{
    [Fact]
    public async Task WithDatabase_After_UseMongo_Works()
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
        var persister = serviceProvider.GetRequiredService<IPersistOutboxRecords>();

        await persister.PersistIndividually(new List<OutboxRecord>(1)
        {
            new(DateTimeOffset.UtcNow, [])
        });

        var outboxCollection = mongoClient.GetDatabase("testdatabase").GetCollection<OutboxRecord>("OutboxRecords");
        var item = await outboxCollection.AsQueryable().SingleOrDefaultAsync();
        item.Should().NotBeNull();
    }
}