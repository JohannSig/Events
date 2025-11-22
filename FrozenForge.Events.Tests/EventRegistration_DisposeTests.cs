using System.Threading.Tasks;
using FrozenForge.Events.Implementations;
using Xunit;

namespace FrozenForge.Events.Tests;

public class EventRegistration_DisposeTests
{
    [Fact]
    public void InvokesDisposeCallbackConstructorMethod()
    {
        var isInvoked = false;

        var subscription = new EventRegistration<TestEvent>();

        subscription.OnDisposed += action => isInvoked = true;

        Assert.False(isInvoked);

        subscription.Dispose();

        Assert.True(isInvoked);
    }

    [Fact]
    public void RemovesSubscriptionFromContainer()
    {
        var container = new EventRegistrationContainer<TestEvent>();

        var subscription = container.Register((@event, cancellationToken) => Task.CompletedTask);

        Assert.Single(container.Registrations);
        Assert.True(container.Registrations.Contains(subscription as EventRegistration<TestEvent>));

        subscription.Dispose();

        Assert.Empty(container.Registrations);
    }

    [Fact]
    public void RemovesContainerFromEventsWithLastRegistration()
    {
        var events = new EventsBase();
        var registration1 = events.Register<TestEvent>(@event => { });
        var registration2 = events.Register<TestEvent2>(@event => { });

        // Two 
        Assert.Equal(2, events.RegistrationContainerByType.Count);

        registration1.Dispose();

        Assert.Single(events.RegistrationContainerByType);

        registration2.Dispose();

        Assert.Empty(events.RegistrationContainerByType);
    }

    private class TestEvent { }

    private class TestEvent2 { }

}
