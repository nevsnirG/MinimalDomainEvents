using MinimalDomainEvents.Contract;
using MinimalDomainEvents.Core;
using MinimalDomainEvents.Dispatcher.Abstractions;

namespace MinimalDomainEvents.Dispatcher.UnitTests;

public class ScopedDomainEventDispatcherTests
{
    [Fact(DisplayName = "Domain events raised using the DomainEventTracker register the events on the deepest active scope")]
    public async Task DomainEventsRaisedWhileScopeIsActiveRegisterOnTheScope()
    {
        var testDispatcher = new TestDispatcher();
        using var scopedDispatcher = new ScopedDomainEventDispatcher(new[] { testDispatcher });
        var topLevelClass = new TopLevelClass(scopedDispatcher, new NestedClass(new DeepestClass()));
        topLevelClass.RaiseDomainEvent();
        await scopedDispatcher.DispatchAndClear();
        testDispatcher.DomainEvents.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Domain events raised while the scoped dispatcher is active are not available once it falls out of scope")]
    public async Task DomainEventsRaisedWhileScopeIsAreNotVisibleOutsideOfScope()
    {
        var testDispatcher = new TestDispatcher();
        using (var scopedDispatcher = new ScopedDomainEventDispatcher(new[] { testDispatcher }))
        {
            var topLevelClass = new TopLevelClass(scopedDispatcher, new NestedClass(new DeepestClass()));
            topLevelClass.RaiseDomainEvent();
            await scopedDispatcher.DispatchAndClear();
            testDispatcher.DomainEvents.Should().HaveCount(1);
        }

        DomainEventTracker.Peek().Should().HaveCount(0);
    }

    [Fact(DisplayName = "Domain events raised in a nested scope are available when dispatching events from a higher scope")]
    public async Task DomainEventsRaisedInANestedScopeAreDispatched()
    {
        var testDispatcher = new TestDispatcher();
        using var scopedDispatcher = new ScopedDomainEventDispatcher(new[] { testDispatcher });
        DomainEventTracker.RaiseDomainEvent(new TestEvent("I was raised in the deepest scope."));
        using (var nestedScope = DomainEventTracker.CreateScope())
        {
            nestedScope.RaiseDomainEvent(new TestEvent("I was raised in the deepest scope."));
        }

        await scopedDispatcher.DispatchAndClear();
        testDispatcher.DomainEvents.Should().HaveCount(2);
    }

    private sealed class TestDispatcher : IDispatchDomainEvents
    {
        public IReadOnlyCollection<IDomainEvent>? DomainEvents { get; private set; }

        public Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents)
        {
            DomainEvents = domainEvents;
            return Task.CompletedTask;
        }
    }

    private class TopLevelClass
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IScopedDomainEventDispatcher _dispatcher;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly NestedClass _nestedDependency;

        public TopLevelClass(IScopedDomainEventDispatcher dispatcher, NestedClass nestedDependency)
        {
            _dispatcher = dispatcher;
            _nestedDependency = nestedDependency;
        }

        public void RaiseDomainEvent()
        {
            _nestedDependency.RaiseDomainEvent();
        }
    }

    private class NestedClass
    {
        private readonly DeepestClass _deepestDependency;

        public NestedClass(DeepestClass deepestDependency)
        {
            _deepestDependency = deepestDependency;
        }

        public void RaiseDomainEvent()
        {
            _deepestDependency.RaiseDomainEvent();
        }
    }

    private class DeepestClass
    {
#pragma warning disable CA1822 // Mark members as static
        public void RaiseDomainEvent()
#pragma warning restore CA1822 // Mark members as static
        {
            DomainEventTracker.RaiseDomainEvent(new TestEvent());
        }
    }

    private sealed record class TestEvent(string Value = "") : IDomainEvent;
}