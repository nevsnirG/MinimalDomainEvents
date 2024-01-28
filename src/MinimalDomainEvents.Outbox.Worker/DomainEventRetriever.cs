using MessagePack;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Outbox.Worker.Abstractions;

namespace MinimalDomainEvents.Outbox.Worker;

internal interface IDomainEventRetriever
{
    Task<IReadOnlyCollection<IDomainEvent>> GetAndMarkAsDispatched(CancellationToken cancellationToken = default);
}

internal sealed class DomainEventRetriever : IDomainEventRetriever
{
    private readonly IRetrieveOutboxRecords _outboxRecordRetriever;

    public DomainEventRetriever(IRetrieveOutboxRecords outboxRecordRetriever)
    {
        _outboxRecordRetriever = outboxRecordRetriever;
    }

    public async Task<IReadOnlyCollection<IDomainEvent>> GetAndMarkAsDispatched(CancellationToken cancellationToken = default)
    {
        var outboxRecords = await _outboxRecordRetriever.GetAndMarkAsDispatched(cancellationToken);
        var options = MessagePack.Resolvers.ContractlessStandardResolver.Options
           .WithResolver(MessagePack.Resolvers.TypelessObjectResolver.Instance);

        var domainEvents = new List<IDomainEvent>(outboxRecords.Count);
        foreach (var outboxRecord in outboxRecords)
        {
            using var memoryStream = new MemoryStream(outboxRecord.MessageData);
            var output = await MessagePackSerializer.Typeless.DeserializeAsync(memoryStream, options, cancellationToken);
            var deserializedDomainEvents = output as IDomainEvent[];

            if (deserializedDomainEvents is not null)
                domainEvents.AddRange(deserializedDomainEvents);
            //TODO - Else?
        }
        return domainEvents;
    }
}