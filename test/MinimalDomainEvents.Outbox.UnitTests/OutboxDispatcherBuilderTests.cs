using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.UnitTests;

public class OutboxDispatcherBuilderTests
{
    [Fact]
    public void Is_Initialized_With_Default_OutboxSettings()
    {
        var sut = new OutboxDispatcherBuilder(Mock.Of<IServiceCollection>());

        sut.OutboxSettings.Should().BeEquivalentTo(OutboxSettings.Default);
    }
}
