using DotNet.Testcontainers.Builders;
using Testcontainers.MongoDb;
using Xunit;

namespace MongoTestContainer;
public sealed class MongoContainerFixture : IAsyncLifetime
{
    public string? ConnectionString => _container?.GetConnectionString();

    private readonly MongoDbContainer _container;

    public MongoContainerFixture()
    {
        _container = new MongoDbBuilder()
            .WithImage("mongo:latest")
            .WithUsername("")
            .WithPassword("")
            .WithCommand("--replSet", "rs0")
            .WithWaitStrategy(Wait.ForUnixContainer())
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await _container.ExecScriptAsync("rs.initiate()");
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
