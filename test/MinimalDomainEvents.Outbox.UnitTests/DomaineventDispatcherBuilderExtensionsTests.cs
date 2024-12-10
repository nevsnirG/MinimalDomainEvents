using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Dispatcher.Abstractions;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.UnitTests;

public class DomaineventDispatcherBuilderExtensionsTests
{
    [Fact]
    public void Given_Configure_Null_Does_Not_Configure_OutboxDispatcherBuilder()
    {
        var serviceCollection = new ServiceCollection();
        var sut = new DomainEventDispatcherBuilder(serviceCollection);

        sut.AddOutbox(null);

        serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IScopedDomainEventDispatcher))
            .Which.ImplementationType.Should().Be(typeof(OutboxDomainEventDispatcher));
    }

    [Fact]
    public void Given_Other_Implementation_Already_Registered_Overrides()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IScopedDomainEventDispatcher, ScopedDomainEventDispatcher>();
        var sut = new DomainEventDispatcherBuilder(serviceCollection);

        sut.AddOutbox(null);

        serviceCollection.Single(sc => sc.ServiceType == typeof(IScopedDomainEventDispatcher))
            .ImplementationType.Should().Be(typeof(OutboxDomainEventDispatcher));
    }

    [Fact]
    public void Given_Configure_Not_Null_Does_Configure_OutboxDispatcherBuilder()
    {
        var serviceCollection = new ServiceCollection();
        var sut = new DomainEventDispatcherBuilder(serviceCollection);
        var serviceDescriptor = new ServiceDescriptor(typeof(object), typeof(object), ServiceLifetime.Transient);

        sut.AddOutbox(b => b.Services.Add(serviceDescriptor));

        serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IScopedDomainEventDispatcher))
            .Which.ImplementationType.Should().Be(typeof(OutboxDomainEventDispatcher));
        serviceCollection.Should().ContainSingle(x => x == serviceDescriptor);
    }
}