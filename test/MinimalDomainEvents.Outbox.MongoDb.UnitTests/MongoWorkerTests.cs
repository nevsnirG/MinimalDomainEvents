﻿using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;
using MongoDB.Driver;

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
    internal async Task Test()
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
        var dispatcher = serviceProvider.GetRequiredService<IDispatchDomainEvents>();

        await dispatcher.Dispatch(new[]
        {
            new TestEvent("SomeValue")
        });

        var transactionProvider = serviceProvider.GetRequiredService<ITransactionProvider>();
        var retriever = serviceProvider.GetRequiredService<IRetrieveOutboxRecords>();
        var cleaner = serviceProvider.GetRequiredService<ICleanupOutboxRecords>();
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
