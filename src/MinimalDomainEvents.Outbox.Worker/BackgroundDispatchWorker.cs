using Microsoft.Extensions.Hosting;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;
using MinimalDomainEvents.Outbox.Worker.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker;
internal sealed class BackgroundDispatchWorker : BackgroundService
{
    private const int Delay = 1000;

    private readonly IOutboxRecordCollectionInitializer _outboxRecordCollectionInitializer;
    private readonly IDomainEventRetriever _domainEventRetriever;
    private readonly ICleanupOutboxRecords _cleanupOutboxRecords;
    private readonly IEnumerable<IDispatchDomainEvents> _dispatchers;

    public BackgroundDispatchWorker(IOutboxRecordCollectionInitializer outboxRecordCollectionInitializer,
                                    IDomainEventRetriever domainEventRetriever,
                                    ICleanupOutboxRecords cleanupOutboxRecords,
                                    IEnumerable<IDispatchDomainEvents> dispatchers)
    {
        _domainEventRetriever = domainEventRetriever;
        _outboxRecordCollectionInitializer = outboxRecordCollectionInitializer;
        _cleanupOutboxRecords = cleanupOutboxRecords;
        _dispatchers = dispatchers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _outboxRecordCollectionInitializer.Initialize(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var domainEvents = await _domainEventRetriever.GetAndMarkAsDispatched(stoppingToken);

            if (domainEvents is not null && domainEvents.Count > 0)
            {
                foreach (var dispatcher in _dispatchers)
                    await dispatcher.Dispatch(domainEvents);
            }

            await _cleanupOutboxRecords.CleanupExpiredOutboxRecords(stoppingToken);

            await Task.Delay(Delay, stoppingToken);
        }
    }
}