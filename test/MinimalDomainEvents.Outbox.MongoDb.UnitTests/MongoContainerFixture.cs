﻿using Testcontainers.MongoDb;

namespace MinimalDomainEvents.Outbox.MongoDb.UnitTests;
[CollectionDefinition("MongoDb Integration")]
public sealed class MognoContainerCollectionFixture : ICollectionFixture<MongoContainerFixture> { }

public sealed class MongoContainerFixture : IAsyncLifetime
{
    public string? ConnectionString => _container?.GetConnectionString();

    private readonly MongoDbContainer _container;

    public MongoContainerFixture()
    {
        _container = new MongoDbBuilder()
            .WithImage("mongo:6.0")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
