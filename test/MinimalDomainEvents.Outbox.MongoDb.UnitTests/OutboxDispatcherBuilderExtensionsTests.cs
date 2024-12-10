using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoTestContainer;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;
[Collection("MongoDb Integration")]
public class OutboxDispatcherBuilderExtensionsTests(MongoContainerFixture fixture) : IAsyncLifetime
{
    private const string DatabaseName = "testdatabase";

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        var mongoClient = CreateClient();
        await mongoClient.DropDatabaseAsync(DatabaseName);
    }

    [Fact]
    public async Task WithDatabase_After_UseMongo_Works()
    {
        var mongoClient = CreateClient();
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddDomainEventDispatcher(dispatcherBuilder =>
        {
            dispatcherBuilder.AddOutbox(outboxBuilder =>
            {
                outboxBuilder.AddMongo(mongoClient);
                outboxBuilder.WithDatabase(DatabaseName);
            });
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var persister = serviceProvider.GetRequiredService<IPersistOutboxRecords>();

        await persister.PersistIndividually(new List<OutboxRecord>(1)
        {
            new(DateTimeOffset.UtcNow, [])
        });

        var outboxCollection = mongoClient.GetDatabase(DatabaseName).GetCollection<OutboxRecord>("OutboxRecords");
        var item = await outboxCollection.AsQueryable().SingleOrDefaultAsync();
        item.Should().NotBeNull();
    }

    private MongoClient CreateClient() => new(fixture.ConnectionString);
}