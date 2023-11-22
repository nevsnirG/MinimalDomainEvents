using FluentAssertions;
using Nevsnirg.DomainEvents.Contract;
using Nevsnirg.DomainEvents.Core;
using System.Runtime.CompilerServices;

namespace Nevsnirg.DomainEvents.Dispatcher.UnitTests;

public class ScopedDomainEventDispatcherTests
{
    [Fact(DisplayName = "Domain events raised using the DomainEventTracker register the events on the deepest active scope")]
    public async Task DomainEventsRegisteredWhileScopeIsActiveRegisterOnTheScope()
    {
        using var scopedDispatcher = new TestDispatcher();
        var topLevelClass = new TopLevelClass(scopedDispatcher, new NestedClass(new DeepestClass()));
        topLevelClass.RaiseDomainEvent();
        await scopedDispatcher.DispatchAndClear();
        scopedDispatcher.DomainEvents.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Domain events raised while the scoped dispatcher is active are not available once it falls out of scope")]
    public async Task DomainEventsRegisteredWhileScopeIsAreNotVisibleOutsideOfScope()
    {
        using (var scopedDispatcher = new TestDispatcher())
        {
            var topLevelClass = new TopLevelClass(scopedDispatcher, new NestedClass(new DeepestClass()));
            topLevelClass.RaiseDomainEvent();
            await scopedDispatcher.DispatchAndClear();
            scopedDispatcher.DomainEvents.Should().HaveCount(1);
        }

        DomainEventTracker.Peek().Should().HaveCount(0);
    }

    private sealed class TestDispatcher : ScopedDomainEventDispatcher
    {
        public IReadOnlyCollection<IDomainEvent>? DomainEvents { get; private set; }

        protected override Task Dispatch(IReadOnlyCollection<IDomainEvent> domainEvents)
        {
            DomainEvents = domainEvents;
            return Task.CompletedTask;
        }
    }

    private class TopLevelClass
    {
        private readonly IDomainEventDispatcher _dispatcher;
        private readonly NestedClass _nestedDependency;

        public TopLevelClass(IDomainEventDispatcher dispatcher, NestedClass nestedDependency)
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
        public void RaiseDomainEvent()
        {
            DomainEventTracker.RaiseDomainEvent(new TestEvent());
        }
    }

    private sealed record class TestEvent(string Value = "") : IDomainEvent;
}