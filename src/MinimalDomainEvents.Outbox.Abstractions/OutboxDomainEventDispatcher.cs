﻿using MessagePack;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Outbox.Abstractions;
internal sealed class OutboxDomainEventDispatcher : IScopedDomainEventDispatcher
{
    public IDomainEventScope? Scope => _scope;

    private IDomainEventScope? _scope;

    private readonly OutboxSettings _settings;
    private readonly IPersistOutboxRecords _domainEventPersister;

    public OutboxDomainEventDispatcher(OutboxSettings settings, IPersistOutboxRecords domainEventPersister)
    {
        _scope = DomainEventTracker.CreateScope();
        _domainEventPersister = domainEventPersister;
        _settings = settings;
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _scope!.RaiseDomainEvent(domainEvent);
    }

    public async Task DispatchAndClear()
    {
        var domainEvents = _scope!.GetAndClearEvents();

        if (domainEvents is null || domainEvents.Count == 0)
            return;

        if (_settings.SendBatched)
        {
            var batchRecord = CreateBatchRecord(domainEvents);
            await _domainEventPersister.PersistBatched(batchRecord);
        }
        else
        {
            var individualRecords = CreateIndividualRecords(domainEvents);
            await _domainEventPersister.PersistIndividually(individualRecords);
        }
    }

    private static OutboxRecord CreateBatchRecord(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        var enqueuedAt = DateTimeOffset.UtcNow;
        var messageData = ToBinary(domainEvents.ToArray());
        return new OutboxRecord(enqueuedAt, messageData);
    }

    private static IReadOnlyCollection<OutboxRecord> CreateIndividualRecords(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        var enqueuedAt = DateTimeOffset.UtcNow;
        var outboxRecords = new List<OutboxRecord>(domainEvents.Count);
        foreach (var domainEvent in domainEvents)
        {
            var outboxRecord = new OutboxRecord(enqueuedAt, ToBinary(new[] { domainEvent }));
            outboxRecords.Add(outboxRecord);
        }
        return outboxRecords;
    }

    private static byte[] ToBinary(object input)
    {
        var options = MessagePack.Resolvers.ContractlessStandardResolver.Options
            .WithResolver(MessagePack.Resolvers.TypelessObjectResolver.Instance)
            .WithSecurity(MessagePackSecurity.UntrustedData)
            ;

        return MessagePackSerializer.Typeless.Serialize(input, options);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (_scope is not null)
        {
            _scope.Dispose();
            _scope = null;
        }
    }
}