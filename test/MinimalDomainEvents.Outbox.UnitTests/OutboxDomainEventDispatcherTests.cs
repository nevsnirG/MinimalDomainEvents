using MessagePack;
using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.UnitTests;

public class OutboxDomainEventDispatcherTests
{
    [Fact]
    public async Task Given_DomainEvents_Null_Throws_ArgumentNullException()
    {
        var sut = new OutboxDomainEventDispatcher(OutboxSettings.Default, null!);

        await FluentActions.Awaiting(() => sut.Dispatch(null!))
                           .Should()
                           .ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Given_DomainEvents_Empty_Returns_Silently()
    {
        var outboxRecordPersisterMock = new Mock<IPersistOutboxRecords>();
        var sut = new OutboxDomainEventDispatcher(OutboxSettings.Default, outboxRecordPersisterMock.Object);

        await sut.Dispatch([]);

        outboxRecordPersisterMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Given_SendBatched_True_Batches_DomainEvents_To_Single_OutboxRecord_And_Verifies_Deserializable()
    {
        // Arrange
        bool callbackSucceeded = false;
        IReadOnlyCollection<IDomainEvent> domainEvents = [new TestEvent("A"), new TestEvent("B")];

        var outboxRecordPersisterMock = new Mock<IPersistOutboxRecords>();
        outboxRecordPersisterMock.Setup(x => x.PersistBatched(It.IsAny<OutboxRecord>()))
            .Callback((OutboxRecord record) =>
            {
                using var memoryStream = new MemoryStream(record.MessageData);
                var domainEvents = MessagePackSerializer.Typeless.Deserialize(memoryStream, SerializerOptions) as IReadOnlyCollection<IDomainEvent>;
                domainEvents.Should().BeEquivalentTo(domainEvents);
                callbackSucceeded = true;
            });

        var sut = new OutboxDomainEventDispatcher(OutboxSettings.Default, outboxRecordPersisterMock.Object);

        // Act
        await sut.Dispatch(domainEvents);

        // Assert
        outboxRecordPersisterMock.Verify(x => x.PersistBatched(It.IsAny<OutboxRecord>()), Times.Once);
        callbackSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Given_SendBatched_False_Persists_OutboxRecords_Individually_And_Verifies_Deserializable()
    {
        // Arrange
        bool callbackSucceeded = false;
        IReadOnlyCollection<TestEvent> domainEvents = [new TestEvent("A"), new TestEvent("B")];

        var outboxRecordPersisterMock = new Mock<IPersistOutboxRecords>();
        outboxRecordPersisterMock.Setup(x => x.PersistIndividually(It.IsAny<IReadOnlyCollection<OutboxRecord>>()))
            .Callback((IReadOnlyCollection<OutboxRecord> records) =>
            {
                for (var i = 0; i < records.Count; i++)
                {
                    using var memoryStream = new MemoryStream(records.ElementAt(i).MessageData);
                    var domainEvent = MessagePackSerializer.Typeless.Deserialize(memoryStream, SerializerOptions) as TestEvent;
                    domainEvent!.PropA.Should().Be(domainEvents.ElementAt(i).PropA);
                }
                callbackSucceeded = true;
            });

        var outboxSettings = new OutboxSettings { SendBatched = false };
        var sut = new OutboxDomainEventDispatcher(outboxSettings, outboxRecordPersisterMock.Object);

        // Act
        await sut.Dispatch(domainEvents);

        // Assert
        outboxRecordPersisterMock.Verify(x => x.PersistIndividually(It.IsAny<IReadOnlyCollection<OutboxRecord>>()), Times.Once);
        callbackSucceeded.Should().BeTrue();
    }

    private static MessagePackSerializerOptions SerializerOptions =>
        MessagePack.Resolvers.ContractlessStandardResolver.Options
            .WithResolver(MessagePack.Resolvers.TypelessObjectResolver.Instance);

    public sealed record class TestEvent(string PropA) : IDomainEvent;
}