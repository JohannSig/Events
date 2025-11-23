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

    private class TestEvent { }
}
