using Microsoft.Extensions.Hosting;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker;
internal sealed class BackgroundDispatchWorker : BackgroundService
{
    private readonly IOutboxRecordCollectionInitializer _outboxRecordCollectionInitializer;
    private readonly ITransactionProvider _transactionFactory;
    private readonly IRetrieveOutboxRecords _retrieveOutboxRecords;

    public BackgroundDispatchWorker(IOutboxRecordCollectionInitializer outboxRecordCollectionInitializer, ITransactionProvider transactionFactory, IRetrieveOutboxRecords retrieveOutboxRecords)
    {
        _retrieveOutboxRecords = retrieveOutboxRecords;
        _outboxRecordCollectionInitializer = outboxRecordCollectionInitializer;
        _transactionFactory = transactionFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _outboxRecordCollectionInitializer.Initialize(stoppingToken);

        while (true)
        {
            using var transaction = await _transactionFactory.NewTransaction(stoppingToken);
            var domainEvents = await _retrieveOutboxRecords.GetAndMarkAsDispatched(stoppingToken);
            //TODO - Dispatch domain events
            //TODO - Clean up Expired domain events.
            await transaction.Commit(stoppingToken);
        }
    }
}