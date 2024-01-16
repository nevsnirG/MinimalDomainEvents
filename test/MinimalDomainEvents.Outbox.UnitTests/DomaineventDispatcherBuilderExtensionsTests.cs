using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Outbox.UnitTests;

public class DomaineventDispatcherBuilderExtensionsTests
{
    [Fact]
    public void Given_Configure_Null_Does_Not_Configure_OutboxDispatcherBuilder()
    {
        var serviceCollectionMock = new Mock<IServiceCollection>();
        var sut = new DomainEventDispatcherBuilder(serviceCollectionMock.Object);

        sut.AddOutbox(null);

        serviceCollectionMock.Verify(x =>
            x.Add(It.Is<ServiceDescriptor>(sd =>
                sd.ServiceType == typeof(IDispatchDomainEvents)
                && sd.ImplementationType == typeof(OutboxDomainEventDispatcher)
                && sd.Lifetime == ServiceLifetime.Scoped)
            ),
            Times.Once);
        serviceCollectionMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Given_Configure_Not_Null_Does_Configure_OutboxDispatcherBuilder()
    {
        var serviceCollectionMock = new Mock<IServiceCollection>();
        var sut = new DomainEventDispatcherBuilder(serviceCollectionMock.Object);
        var serviceDescriptor = new ServiceDescriptor(typeof(object), typeof(object), ServiceLifetime.Transient);

        sut.AddOutbox(b => b.Services.Add(serviceDescriptor));

        serviceCollectionMock.Verify(x =>
            x.Add(It.Is<ServiceDescriptor>(sd =>
                sd.ServiceType == typeof(IDispatchDomainEvents)
                && sd.ImplementationType == typeof(OutboxDomainEventDispatcher)
                && sd.Lifetime == ServiceLifetime.Scoped)
            ),
            Times.Once);
        serviceCollectionMock.Verify(x =>
            x.Add(serviceDescriptor),
            Times.Once);
        serviceCollectionMock.VerifyNoOtherCalls();
    }
}