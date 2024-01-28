using Microsoft.Extensions.Hosting;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker;
internal sealed class BackgroundDispatchWorker : BackgroundService
{
    private readonly IOutboxRecordCollectionInitializer _outboxRecordCollectionInitializer;
    private readonly ITransactionProvider _transactionFactory;
    private readonly IDomainEventRetriever _domainEventRetriever;
    private readonly ICleanupOutboxRecords _cleanupOutboxRecords;

    public BackgroundDispatchWorker(IOutboxRecordCollectionInitializer outboxRecordCollectionInitializer,
                                    ITransactionProvider transactionFactory,
                                    IDomainEventRetriever domainEventRetriever,
                                    ICleanupOutboxRecords cleanupOutboxRecords)
    {
        _domainEventRetriever = domainEventRetriever;
        _outboxRecordCollectionInitializer = outboxRecordCollectionInitializer;
        _transactionFactory = transactionFactory;
        _cleanupOutboxRecords = cleanupOutboxRecords;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _outboxRecordCollectionInitializer.Initialize(stoppingToken);

        while (true)
        {
            using var transaction = await _transactionFactory.NewTransaction(stoppingToken);
            var domainEvents = await _domainEventRetriever.GetAndMarkAsDispatched(stoppingToken);
            //TODO - Dispatch domain events
            await _cleanupOutboxRecords.CleanupExpiredOutboxRecords(stoppingToken);
            await transaction.Commit(stoppingToken);
        }
    }
}