namespace MinimalDomainEvents.Core.UnitTests;
public class DomainEventScopeTests
{
    [Fact(DisplayName = "Domain events raised inside of a scope are accessible inside of that scope")]
    public void DomainEventsAreTrackedInsideOfScopes()
    {
        using var scope = DomainEventTracker.CreateScope();
        scope.RaiseDomainEvent(new TestEvent());
        var createdEvents = scope.GetAndClearEvents();
        createdEvents.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Domain events raised inside of a scope are not accessible outside of that scope")]
    public void DomainEventsAreNotTrackedOutsideOfScopes()
    {
        using (var scope = DomainEventTracker.CreateScope())
        {
            scope.RaiseDomainEvent(new TestEvent());
        }

        var createdEvents = DomainEventTracker.GetAndClearEvents();
        createdEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "Peek on the scope returns only the domain events registered on that scope")]
    public void PeekOnScopeReturnsDomainEventsOnDeepestScope()
    {
        using (var scope = DomainEventTracker.CreateScope())
        {
            scope.RaiseDomainEvent(new TestEvent("I am raised in the top scope."));
            using (var deepestScope = DomainEventTracker.CreateScope())
            {
                deepestScope.RaiseDomainEvent(new TestEvent("I am raised in the deepest scope."));

                scope.Peek().Should().ContainSingle()
                    .Which.Should().BeOfType<TestEvent>()
                    .Which.Value.Should().Be("I am raised in the top scope.");

                deepestScope.Peek().Should().ContainSingle()
                    .Which.Should().BeOfType<TestEvent>()
                    .Which.Value.Should().Be("I am raised in the deepest scope.");
            }
        }
    }

    [Fact(DisplayName = "Domain events raised on a higher scope in a deeper scope are only  registered on that scope")]
    public void RaiseDomainEventOnScopeReturnsOnlyDomainEventsRaisedOnThatScope()
    {
        using (var scope = DomainEventTracker.CreateScope())
        {
            using (var deepestScope = DomainEventTracker.CreateScope())
            {
                scope.RaiseDomainEvent(new TestEvent("I am raised in the top scope."));

                scope.Peek().Should().ContainSingle()
                    .Which.Should().BeOfType<TestEvent>()
                    .Which.Value.Should().Be("I am raised in the top scope.");

                deepestScope.Peek().Should().BeEmpty();
            }
        }
    }

    [Fact(DisplayName = "Peek on the DomainEventTracker returns the domain events registered on the current deepest scope")]
    public void PeekReturnsDomainEventsOnDeepestScope()
    {
        using (var scope = DomainEventTracker.CreateScope())
        {
            scope.RaiseDomainEvent(new TestEvent("I am raised in the top scope."));
            using (var deepestScope = DomainEventTracker.CreateScope())
            {
                deepestScope.RaiseDomainEvent(new TestEvent("I am raised in the deepest scope."));
                DomainEventTracker.Peek().Should().ContainSingle()
                    .Which.Should().BeOfType<TestEvent>()
                    .Which.Value.Should().Be("I am raised in the deepest scope.");
            }
        }
    }

    private sealed record class TestEvent(string Value = "") : IDomainEvent;
}
