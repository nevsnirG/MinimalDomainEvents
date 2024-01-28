using MessagePack;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox;
internal sealed class OutboxDomainEventDispatcher : IDispatchDomainEvents
{
    private readonly OutboxSettings _settings;
    private readonly IPersistOutboxRecords _domainEventPersister;

    public OutboxDomainEventDispatcher(OutboxSettings settings, IPersistOutboxRecords domainEventPersister)
    {
        _domainEventPersister = domainEventPersister;
        _settings = settings;
    }

    public async Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);
        if (domainEvents.Count == 0)
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

    private static byte[] ToBinary<T>(T[] input)
    {
        var options = MessagePack.Resolvers.ContractlessStandardResolver.Options
            .WithResolver(MessagePack.Resolvers.TypelessObjectResolver.Instance);
        
        return MessagePackSerializer.Typeless.Serialize(input, options);
    }
}