using FluentAssertions;
using MinimalDomainEvents.Contract;

namespace MinimalDomainEvents.Core.UnitTests;

public class DomainEventTrackerTests
{
    [Fact(DisplayName = "Domain events raised inside of a scope are accessible inside of that scope")]
    public void DomainEventsAreTrackedInsideOfScopes()
    {
        using var scope = DomainEventTracker.CreateScope();
        DomainEventTracker.RaiseDomainEvent(new TestEvent());
        var createdEvents = DomainEventTracker.GetAndClearEvents();
        createdEvents.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Domain events raised inside of a scope are not accessible outside of that scope")]
    public void DomainEventsAreNotTrackedOutsideOfScopes()
    {
        using (var scope = DomainEventTracker.CreateScope())
        {
            DomainEventTracker.RaiseDomainEvent(new TestEvent());
        }

        var createdEvents = DomainEventTracker.GetAndClearEvents();
        createdEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "Domain events raised inside of a nested scope are not accessible in the parent scope.")]
    public void DomainEventsRaisedInNestedScopesAreAccessibleInTheParentScope()
    {
        using (var scope = DomainEventTracker.CreateScope())
        {
            DomainEventTracker.RaiseDomainEvent(new TestEvent("I was raised in the top scope."));
            using (var nestedScope = DomainEventTracker.CreateScope())
            {
                DomainEventTracker.RaiseDomainEvent(new TestEvent("I was raised in the nested scope."));
                using (var evenMoreNestedScope = DomainEventTracker.CreateScope())
                {
                    DomainEventTracker.RaiseDomainEvent(new TestEvent("I was raised in the deepest scope."));
                    DomainEventTracker.Peek().Should().HaveCount(1)
                        .And.Subject.Single().Should().BeOfType<TestEvent>()
                        .Which.Value.Should().Be("I was raised in the deepest scope.");
                }

                DomainEventTracker.RaiseDomainEvent(new TestEvent("I was also raised in the nested scope."));
                DomainEventTracker.Peek().Should().HaveCount(2)
                        .And.Subject.First().Should().BeOfType<TestEvent>()
                        .Which.Value.Should().Be("I was raised in the nested scope.");
                DomainEventTracker.Peek().Should().HaveCount(2)
                        .And.Subject.Last().Should().BeOfType<TestEvent>()
                        .Which.Value.Should().Be("I was also raised in the nested scope.");
            }

            DomainEventTracker.Peek().Should().HaveCount(1)
                        .And.Subject.Single().Should().BeOfType<TestEvent>()
                        .Which.Value.Should().Be("I was raised in the top scope.");
        }

        DomainEventTracker.Peek().Should().BeEmpty();
    }

    [Fact(DisplayName = "A higher scope disposed in a deeper scope will cascade disposing of all deeper scopes.")]
    public void DisposingAHigherScopeInADeeperScopeDisposesAllDeeperScope()
    {
        using var topScope = DomainEventTracker.CreateScope();
        topScope.RaiseDomainEvent(new TestEvent("I was raised in the top scope."));

        using var deeperScope = DomainEventTracker.CreateScope();
        deeperScope.RaiseDomainEvent(new TestEvent("I was raised in the deeper scope."));

        using var evenDeeperScope = DomainEventTracker.CreateScope();
        evenDeeperScope.RaiseDomainEvent(new TestEvent("I was raised in the even deeper scope."));

        using var deepestScope = DomainEventTracker.CreateScope();
        deepestScope.RaiseDomainEvent(new TestEvent("I was raised in the deepest scope."));

        deeperScope.Dispose();

        DomainEventTracker.Peek().Should().HaveCount(1)
                    .And.Subject.Single().Should().BeOfType<TestEvent>()
                    .Which.Value.Should().Be("I was raised in the top scope.");
    }

    private sealed record class TestEvent(string Value = "") : IDomainEvent;
}