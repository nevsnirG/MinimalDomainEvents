using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker.UnitTests;

public class BackgroundDispatchWorkerTests
{
    [Fact]
    public async Task Worker_RetrievesDomainEventsFromOutbox_AndDispatchesThem_AndCommitsTransaction()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var collectionInitializer = new Mock<IOutboxRecordCollectionInitializer>();
        var transactionProviderMock = new Mock<ITransactionProvider>();
        var domainEventRetrieverMock = new Mock<IDomainEventRetriever>();
        var outboxCleanerMock = new Mock<ICleanupOutboxRecords>();
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
                                                                   outboxCleanerMock.Object,
                                                                   [domainEventDispatcherMock.Object]);


        try
        {
            await sut.StartAsync(cancellationToken);
        }
        catch (TaskCanceledException) { }

        collectionInitializer.Verify(x => x.Initialize(It.IsAny<CancellationToken>()), Times.Once);
        domainEventRetrieverMock.Verify(x => x.GetAndMarkAsDispatched(It.IsAny<CancellationToken>()), Times.Once);
        domainEventDispatcherMock.Verify(x => x.Dispatch(It.Is<IReadOnlyCollection<IDomainEvent>>(c => c.Single()  == domainEvent)), Times.Once);
        outboxCleanerMock.Verify(x => x.CleanupExpiredOutboxRecords(It.IsAny<CancellationToken>()), Times.Once);
        outboxTransactionMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }
}