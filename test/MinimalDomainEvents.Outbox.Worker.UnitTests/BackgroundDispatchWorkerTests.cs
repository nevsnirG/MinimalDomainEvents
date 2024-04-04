using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Dispatcher.MediatR;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.MongoDb;
using MongoDB.Driver;
using MongoTestContainer;

namespace MinimalDomainEvents.Outbox.Worker.UnitTests;
[Collection("MongoDb Integration")]
public class BackgroundDispatchWorkerTests(MongoContainerFixture fixture) : IAsyncLifetime
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
    public async Task Worker_RetrievesDomainEventsFromOutbox_AndDispatchesThem_AndCommitsTransaction()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var collectionInitializer = new Mock<IOutboxRecordCollectionInitializer>();
        var transactionProviderMock = new Mock<ITransactionProvider>();
        var domainEventRetrieverMock = new Mock<IDomainEventRetriever>();
        var domainEventDispatcherMock = new Mock<IDispatchDomainEvents>();
        var outboxTransactionMock = new Mock<IOutboxTransaction>();
        var domainEvent = Mock.Of<IDomainEvent>();

        transactionProviderMock.Setup(x => x.NewTransaction(It.IsAny<CancellationToken>()))
            .ReturnsAsync(outboxTransactionMock.Object);

        outboxTransactionMock.Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .Callback(cancellationTokenSource.Cancel);

        domainEventRetrieverMock.Setup(x => x.GetAndMarkAsDispatched(It.IsAny<CancellationToken>()))
            .ReturnsAsync([domainEvent]);

        var sut = new BackgroundDispatchWorker(collectionInitializer.Object,
                                                                   transactionProviderMock.Object,
                                                                   domainEventRetrieverMock.Object,
                                                                   [domainEventDispatcherMock.Object]);

        try
        {
            await sut.StartAsync(cancellationToken);
        }
        catch (TaskCanceledException) { }

        collectionInitializer.Verify(x => x.Initialize(It.IsAny<CancellationToken>()), Times.Once);
        domainEventRetrieverMock.Verify(x => x.GetAndMarkAsDispatched(It.IsAny<CancellationToken>()), Times.Once);
        domainEventDispatcherMock.Verify(x => x.Dispatch(It.Is<IReadOnlyCollection<IDomainEvent>>(c => c.Single() == domainEvent)), Times.Once);
        outboxTransactionMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Given_MongoPersistence_Outbox_CreatesIndexes_And_DispatchesItems()
    {
        var mongoClient = CreateClient();
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(GetType());
        });
        serviceCollection.RemoveAll<INotificationHandler<TestDomainEvent>>();
        serviceCollection.AddSingleton<INotificationHandler<TestDomainEvent>, TestHandler>();

        serviceCollection.AddDomainEventDispatcher(dispatcherBuilder =>
        {
            dispatcherBuilder.AddMediatorDispatcher();
            dispatcherBuilder.AddOutbox(outboxBuilder =>
            {
                outboxBuilder.AddMongo(mongoClient);
                outboxBuilder.WithDatabase(DatabaseName);
                outboxBuilder.WithHostingDispatcher();
            });
        });

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var outboxDispatcher = serviceProvider.GetRequiredService<IScopedDomainEventDispatcher>();
        outboxDispatcher.Should().BeOfType<OutboxDomainEventDispatcher>();
        outboxDispatcher.RaiseDomainEvent(new TestDomainEvent("TestValue"));

        var backgroundWorker = serviceProvider.GetRequiredService<IHostedService>() as BackgroundDispatchWorker;
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromHours(5));
        
        await backgroundWorker!.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(1000); //Wait for the collection to initialize.
        await outboxDispatcher.DispatchAndClear(); //Persist the domain events in the OutboxRecords collection.
        await Task.Delay(2000); //Wait for the worker to dispatch the OutboxRecords.

        var mongoDatabase = mongoClient.GetDatabase(DatabaseName);
        var outboxRecordCollection = mongoDatabase.GetCollection<OutboxRecord>("OutboxRecords");
        var indexes = await (await outboxRecordCollection.Indexes.ListAsync()).ToListAsync();
        indexes.Should().HaveCount(3); //TODO - Assert them all.
        outboxRecordCollection.AsQueryable().All(or => or.DispatchedAt != null).Should().BeTrue();

        var testDomainEventHandler = serviceProvider.GetRequiredService<INotificationHandler<TestDomainEvent>>() as TestHandler;
        testDomainEventHandler!.Value.Should().Be("TestValue");
    }

    public sealed record class TestDomainEvent(string Value) : IDomainEvent, INotification;

    public class TestHandler : INotificationHandler<TestDomainEvent>
    {
        public string Value { get; private set; } = string.Empty;

        public Task Handle(TestDomainEvent notification, CancellationToken cancellationToken)
        {
            Value = notification.Value;
            return Task.CompletedTask;
        }
    }

    private MongoClient CreateClient() => new(fixture.ConnectionString);
}