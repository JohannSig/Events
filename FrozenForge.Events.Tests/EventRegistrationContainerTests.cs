// PSEUDOCODE / PLAN (detailed):
// 1. Create tests targeting EventRegistrationContainer<TEvent> behaviors:
//    - Registering a callback and triggering the event should invoke the callback with the provided event value.
//    - Disposing an individual registration should prevent that callback from being invoked on subsequent triggers.
//    - Disposing the container should prevent further registrations and should allow TriggerAsync to be a no-op.
//    - Multiple registered callbacks should run (concurrently) and all complete when TriggerAsync completes.
// 2. For each test:
//    - Instantiate EventRegistrationContainer<T> using a NullLogger.
//    - Use TaskCompletionSource to observe callback invocation asynchronously and avoid timing/race flakiness.
//    - Assert expected outcomes (value received, callbacks not invoked, exceptions thrown).
//    - Dispose registrations and container when appropriate to avoid affecting other tests.
// 3. Use xUnit [Fact] for tests and simple synchronous assertions for expected exceptions or boolean flags.
// 4. Keep tests isolated and deterministic (use RunContinuationsAsynchronously for TCS).
//
// Note: This test file assumes internal visibility is permitted (InternalsVisibleTo) or that tests
// are part of the same assembly for accessing internal types like EventRegistrationContainer<TEvent>.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using FrozenForge.Events.Implementations;

namespace FrozenForge.Events.Tests;

public class EventRegistrationContainerTests
{
    [Fact]
    public async Task RegisterAndTrigger_InvokesCallback()
    {
        var container = new EventRegistrationContainer<string>(NullLogger<EventRegistrationContainer<string>>.Instance);

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        var registration = container.Register(async (s, ct) =>
        {
            // Capture the value and complete the TCS
            tcs.TrySetResult(s);
            await Task.CompletedTask;
        });

        await container.TriggerAsync("hello", CancellationToken.None);

        var result = await tcs.Task;
        Assert.Equal("hello", result);

        registration.Dispose();
        container.Dispose();
    }

    [Fact]
    public async Task DisposedRegistration_NotInvokedAfterDispose()
    {
        var container = new EventRegistrationContainer<string>(NullLogger<EventRegistrationContainer<string>>.Instance);

        var called = false;
        var registration = container.Register(async (s, ct) =>
        {
            called = true;
            await Task.CompletedTask;
        });

        // Dispose individual registration
        registration.Dispose();

        Assert.False(called);

        await container.TriggerAsync("x", CancellationToken.None);

        Assert.False(called);

        container.Dispose();
    }

    [Fact]
    public void DisposeContainer_PreventsRegisteringAndThrowsOnRegister()
    {
        var container = new EventRegistrationContainer<string>(NullLogger<EventRegistrationContainer<string>>.Instance);

        // Dispose the container
        container.Dispose();

        // Registering after disposal should throw ObjectDisposedException
        Assert.Throws<ObjectDisposedException>(() => container.Register((s, ct) => Task.CompletedTask));
    }

    [Fact]
    public async Task MultipleCallbacks_AllInvokedWhenTriggered()
    {
        var container = new EventRegistrationContainer<int>(NullLogger<EventRegistrationContainer<int>>.Instance);

        var tcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var tcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        container.Register(async (i, ct) =>
        {
            // Simulate some async work
            await Task.Delay(50, ct);
            tcs1.TrySetResult(true);
        });

        container.Register(async (i, ct) =>
        {
            // Simulate some async work
            await Task.Delay(50, ct);
            tcs2.TrySetResult(true);
        });

        // Trigger and wait for both callbacks to complete
        await container.TriggerAsync(1, CancellationToken.None);

        await Task.WhenAll(tcs1.Task, tcs2.Task);

        Assert.True(tcs1.Task.Result);
        Assert.True(tcs2.Task.Result);

        container.Dispose();
    }
}