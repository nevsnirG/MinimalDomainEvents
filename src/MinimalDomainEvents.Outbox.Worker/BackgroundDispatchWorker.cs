using Microsoft.Extensions.Hosting;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker;
internal sealed class BackgroundDispatchWorker : BackgroundService
{
    private readonly IOutboxRecordCollectionInitializer _outboxRecordCollectionInitializer;
    private readonly ITransactionProvider _transactionFactory;
    private readonly IRetrieveOutboxRecords _retrieveOutboxRecords;
    private readonly ICleanupOutboxRecords _cleanupOutboxRecords;

    public BackgroundDispatchWorker(IOutboxRecordCollectionInitializer outboxRecordCollectionInitializer,
                                    ITransactionProvider transactionFactory,
                                    IRetrieveOutboxRecords retrieveOutboxRecords,
                                    ICleanupOutboxRecords cleanupOutboxRecords)
    {
        _retrieveOutboxRecords = retrieveOutboxRecords;
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
            var domainEvents = await _retrieveOutboxRecords.GetAndMarkAsDispatched(stoppingToken);
            //TODO - Dispatch domain events
            await _cleanupOutboxRecords.CleanupExpiredOutboxRecords(stoppingToken);
            await transaction.Commit(stoppingToken);
        }
    }
}