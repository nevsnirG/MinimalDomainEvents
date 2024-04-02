using MessagePack;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.UnitTests;

public class OutboxDomainEventDispatcherTests
{
    [Fact]
    public async Task Given_DomainEvents_Null_DoesNotDoAnything()
    {
        var outboxRecordPersisterMock = new Mock<IPersistOutboxRecords>();
        var sut = new OutboxDomainEventDispatcher(OutboxSettings.Default, outboxRecordPersisterMock.Object);

        await sut.DispatchAndClear();

        outboxRecordPersisterMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Given_SendBatched_True_Batches_DomainEvents_To_Single_OutboxRecord_And_Verifies_Deserializable()
    {
        // Arrange
        bool callbackSucceeded = false;
        IReadOnlyCollection<IDomainEvent> domainEvents = [new TestEvent("A"), new TestEvent("B")];

        var outboxRecordPersisterMock = new Mock<IPersistOutboxRecords>();
        outboxRecordPersisterMock.Setup(x => x.PersistBatched(It.IsAny<OutboxRecord>(), default))
            .Callback((OutboxRecord record, CancellationToken _) =>
            {
                using var memoryStream = new MemoryStream(record.MessageData);
                var domainEvents = MessagePackSerializer.Typeless.Deserialize(memoryStream, SerializerOptions) as IReadOnlyCollection<IDomainEvent>;
                domainEvents.Should().BeEquivalentTo(domainEvents);
                callbackSucceeded = true;
            });

        var sut = new OutboxDomainEventDispatcher(OutboxSettings.Default, outboxRecordPersisterMock.Object);
        foreach (var domainEvent in domainEvents)
        {
            DomainEventTracker.RaiseDomainEvent(domainEvent);
        }

        // Act
        await sut.DispatchAndClear();

        // Assert
        outboxRecordPersisterMock.Verify(x => x.PersistBatched(It.IsAny<OutboxRecord>(), default), Times.Once);
        callbackSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Given_SendBatched_False_Persists_OutboxRecords_Individually_And_Verifies_Deserializable()
    {
        // Arrange
        bool callbackSucceeded = false;
        IReadOnlyCollection<TestEvent> domainEvents = [new TestEvent("A"), new TestEvent("B")];

        var outboxRecordPersisterMock = new Mock<IPersistOutboxRecords>();
        outboxRecordPersisterMock.Setup(x => x.PersistIndividually(It.IsAny<IReadOnlyCollection<OutboxRecord>>(), default))
            .Callback((IReadOnlyCollection<OutboxRecord> records, CancellationToken _) =>
            {
                for (var i = 0; i < records.Count; i++)
                {
                    using var memoryStream = new MemoryStream(records.ElementAt(i).MessageData);
                    var domainEvent = MessagePackSerializer.Typeless.Deserialize(memoryStream, SerializerOptions) as TestEvent;
                    domainEvent.Should().NotBeNull();
                    domainEvent!.PropA.Should().Be(domainEvents.ElementAt(i).PropA);
                }
                callbackSucceeded = true;
            });

        var outboxSettings = new OutboxSettings { SendBatched = false };
        var sut = new OutboxDomainEventDispatcher(outboxSettings, outboxRecordPersisterMock.Object);
        foreach (var domainEvent in domainEvents)
        {
            DomainEventTracker.RaiseDomainEvent(domainEvent);
        }

        // Act
        await sut.DispatchAndClear();

        // Assert
        outboxRecordPersisterMock.Verify(x => x.PersistIndividually(It.IsAny<IReadOnlyCollection<OutboxRecord>>(), default), Times.Once);
        callbackSucceeded.Should().BeTrue();
    }

    private static MessagePackSerializerOptions SerializerOptions =>
        MessagePack.Resolvers.ContractlessStandardResolver.Options
            .WithResolver(MessagePack.Resolvers.TypelessObjectResolver.Instance)
            .WithSecurity(MessagePackSecurity.UntrustedData)
        ;

    public sealed record class TestEvent(string PropA) : IDomainEvent;
}