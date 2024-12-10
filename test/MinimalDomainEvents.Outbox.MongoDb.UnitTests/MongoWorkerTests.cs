using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;
using MongoDB.Driver;
using MongoTestContainer;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;
[Collection("MongoDb Integration")]
public class MongoWorkerTests(MongoContainerFixture fixture) : IAsyncLifetime
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
    internal async Task Using_Transactions_Persists_OutboxRecords_Within_Same_Transaction()
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
            dispatcherBuilder.Services.AddSingleton(Mock.Of<IDispatchDomainEvents>());
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IScopedDomainEventDispatcher>();

        dispatcher.RaiseDomainEvent(new TestEvent("SomeValue"));
        await dispatcher.DispatchAndClear();

        var transactionProvider = serviceProvider.GetRequiredService<ITransactionProvider>();
        var retriever = serviceProvider.GetRequiredService<IRetrieveOutboxRecords>();
        using var transaction = await transactionProvider.NewTransaction();

        var outboxRecords = await retriever.GetAndMarkAsDispatched();
        await transaction.Commit();

        var outboxRecord = outboxRecords.Single().Should().BeOfType<OutboxRecord>().Which;
        var domainEvents = await GetDomainEventsFromOutboxRecord(outboxRecord);
        domainEvents.Should().ContainSingle()
                    .Which.Should().BeOfType<TestEvent>()
                    .Which.SomeProperty.Should().Be("SomeValue");

        var mongoCollection = mongoClient.GetDatabase(DatabaseName).GetCollection<OutboxRecord>("OutboxRecords");
        outboxRecords = mongoCollection.AsQueryable().ToList();
        outboxRecords.Should().ContainSingle()
                     .Which.DispatchedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static async Task<IDomainEvent[]> GetDomainEventsFromOutboxRecord(OutboxRecord outboxRecord)
    {
        var options = MessagePack.Resolvers.ContractlessStandardResolver.Options
            .WithResolver(MessagePack.Resolvers.TypelessObjectResolver.Instance);

        using var memoryStream = new MemoryStream(outboxRecord.MessageData);
        var domainEvents = await  MessagePackSerializer.Typeless.DeserializeAsync(memoryStream, options);
        return (domainEvents as IDomainEvent[])!;
    }

    private MongoClient CreateClient() => new(fixture.ConnectionString);

    private sealed record class TestEvent(string SomeProperty) : IDomainEvent;
}
