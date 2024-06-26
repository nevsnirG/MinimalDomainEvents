﻿using Microsoft.Extensions.Hosting;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;
using System.Diagnostics;

namespace MinimalDomainEvents.Outbox.Worker;
internal sealed class BackgroundDispatchWorker : BackgroundService
{
    private const int Delay = 1000;

    private readonly IOutboxRecordCollectionInitializer _outboxRecordCollectionInitializer;
    private readonly ITransactionProvider _transactionFactory;
    private readonly IDomainEventRetriever _domainEventRetriever;
    private readonly IEnumerable<IDispatchDomainEvents> _dispatchers;

    public BackgroundDispatchWorker(IOutboxRecordCollectionInitializer outboxRecordCollectionInitializer,
                                    ITransactionProvider transactionFactory,
                                    IDomainEventRetriever domainEventRetriever,
                                    IEnumerable<IDispatchDomainEvents> dispatchers)
    {
        _domainEventRetriever = domainEventRetriever;
        _outboxRecordCollectionInitializer = outboxRecordCollectionInitializer;
        _transactionFactory = transactionFactory;
        _dispatchers = dispatchers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _outboxRecordCollectionInitializer.Initialize(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var transaction = await _transactionFactory.NewTransaction(stoppingToken);
                var domainEvents = await _domainEventRetriever.GetAndMarkAsDispatched(stoppingToken);

                if (domainEvents is not null && domainEvents.Count > 0)
                {
                    foreach (var dispatcher in _dispatchers)
                        await dispatcher.Dispatch(domainEvents);
                }

                await transaction.Commit(stoppingToken);

                await Task.Delay(Delay, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("BackgroundDispatchWorker stopped. Reason: TaskCanceledException.");
        }
    }
}