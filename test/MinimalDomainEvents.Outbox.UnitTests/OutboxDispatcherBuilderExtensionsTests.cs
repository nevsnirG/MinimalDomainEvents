using Microsoft.Extensions.DependencyInjection;
using MinimalDomainEvents.Outbox.Abstractions;

namespace MinimalDomainEvents.Outbox.UnitTests;

public class OutboxDispatcherBuilderExtensionsTests
{
    [Fact]
    public void WithDatabase_Sets_OutboxSettings_DatabaseName()
    {
        var sut = new OutboxDispatcherBuilder(Mock.Of<IServiceCollection>());

        sut.WithDatabase("testdatabase");

        sut.OutboxSettings.DatabaseName.Should().Be("testdatabase");
    }
}
