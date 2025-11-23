using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using FrozenForge.Events;
using FrozenForge.Events.Implementations;
using Microsoft.Extensions.Logging.Abstractions;

namespace FrozenForge.Events.Tests;

public class EventsBaseTests
{
    private static ILoggerFactory CreateLoggerFactory() => NullLoggerFactory.Instance;

    private record TestEvent(int Value);

    [Fact]
    public async Task Register_Action_Handler_IsInvoked()
    {
        using var loggerFactory = CreateLoggerFactory();
        using var events = new EventsBase(loggerFactory);

        var invoked = false;
        int captured = 0;

        using var registration = events.Register<TestEvent>(e =>
        {
            invoked = true;
            captured = e.Value;
        });

        await events.TriggerAsync(new TestEvent(5));

        Assert.True(invoked);
        Assert.Equal(5, captured);
    }

    [Fact]
    public async Task Register_FuncTask_Handler_IsInvoked()
    {
        using var loggerFactory = CreateLoggerFactory();
        using var events = new EventsBase(loggerFactory);

        var invoked = false;
        int captured = 0;

        using var registration = events.Register<TestEvent>(e =>
        {
            invoked = true;
            captured = e.Value;
            return Task.CompletedTask;
        });

        await events.TriggerAsync(new TestEvent(7));

        Assert.True(invoked);
        Assert.Equal(7, captured);
    }

    [Fact]
    public async Task Register_WithCancellationToken_Handler_ReceivesToken()
    {
        using var loggerFactory = CreateLoggerFactory();
        using var events = new EventsBase(loggerFactory);

        var invoked = false;
        int captured = 0;

        using var registration = events.Register<TestEvent>(async (e, ct) =>
        {
            // ensure token is passed and not canceled in normal trigger
            if (ct.IsCancellationRequested) throw new OperationCanceledException();
            invoked = true;
            captured = e.Value;
            await Task.CompletedTask;
        });

        await events.TriggerAsync(new TestEvent(11), CancellationToken.None);

        Assert.True(invoked);
        Assert.Equal(11, captured);
    }

    [Fact]
    public async Task Multiple_Handlers_AllInvoked()
    {
        using var loggerFactory = CreateLoggerFactory();
        using var events = new EventsBase(loggerFactory);

        var count = 0;

        using var reg1 = events.Register<TestEvent>(e => { count++; });
        using var reg2 = events.Register<TestEvent>(e => { count++; });

        await events.TriggerAsync(new TestEvent(1));

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task DisposeRegistration_HandlerNotInvokedAfterDispose()
    {
        using var loggerFactory = CreateLoggerFactory();
        using var events = new EventsBase(loggerFactory);

        var invoked = false;

        var registration = events.Register<TestEvent>(e => { invoked = true; });

        registration.Dispose();

        await events.TriggerAsync(new TestEvent(2));

        Assert.False(invoked);
    }

    [Fact]
    public async Task DisposeEventsBase_RemovesRegistrations()
    {
        using var loggerFactory = CreateLoggerFactory();
        var events = new EventsBase(loggerFactory);

        var invoked = false;

        using var registration = events.Register<TestEvent>(e => { invoked = true; });

        events.Dispose();

        // Trigger after disposal should be a no-op and should not throw
        await events.TriggerAsync(new TestEvent(3));

        Assert.False(invoked);

        // calling Dispose again should be safe
        events.Dispose();
    }

    [Fact]
    public async Task Trigger_NoRegistrations_Completes()
    {
        using var loggerFactory = CreateLoggerFactory();
        using var events = new EventsBase(loggerFactory);

        // No registrations for this event type
        await events.TriggerAsync(new TestEvent(99));
        // If we reach here without exception, test passes
        Assert.True(true);
    }
}