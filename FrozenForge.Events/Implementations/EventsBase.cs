using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("FrozenForge.Events.Tests")]
namespace FrozenForge.Events.Implementations;

public class EventsBase(ILogger<EventsBase> logger) : IEvents
{
    internal ConcurrentDictionary<Type, IEventRegistrationContainer> RegistrationContainerByType { get; set; } = new ConcurrentDictionary<Type, IEventRegistrationContainer>();
    public ILogger<EventsBase> Logger { get; } = logger;

    private bool isDisposed;

    public IDisposable Register<TEvent>(Action<TEvent> callback)
        => Register<TEvent>(@event => { callback(@event); return Task.CompletedTask; });

    public IDisposable Register<TEvent>(Func<TEvent, Task> callback)
        => Register<TEvent>((@event, cancellationToken) => callback(@event));

    public IDisposable Register<TEvent>(Func<TEvent, CancellationToken, Task> callback)
    {
        if (!RegistrationContainerByType.TryGetValue(typeof(TEvent), out var container))
        {
            container = new EventRegistrationContainer<TEvent>();
            container.OnDisposed += OnRegistrationContainerDisposed;
            if (!RegistrationContainerByType.TryAdd(typeof(TEvent), container))
            {
                throw new Exception("Failed to add event registration container.");
            }
        }

        return ((IEventRegistrationContainer<TEvent>)container).Register(callback);
    }

    public Task TriggerAsync<TEvent>(TEvent @event) => TriggerAsync(@event, CancellationToken.None);

    public Task TriggerAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        if (RegistrationContainerByType.TryGetValue(typeof(TEvent), out var container))
        {
            return ((IEventRegistrationContainer<TEvent>)container).TriggerAsync(@event, cancellationToken);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        this.Dispose(true);

        GC.SuppressFinalize(this);
    }

    public void Dispose(bool isDisposing)
    {
        if (!isDisposed)
        {
            isDisposed = true;

            if (isDisposing)
            {
                foreach (var containerKey in RegistrationContainerByType.Keys.ToList())
                {
                    if (!RegistrationContainerByType.TryRemove(containerKey, out var container))
                    {
                        this.Logger.LogWarning("Failed to remove event registration container for event type {EventType} during disposal.", containerKey);
                        continue;
                    }

                    container.OnDisposed -= OnRegistrationContainerDisposed;
                    container.Dispose();
                }
            }
        }
    }

    private void OnRegistrationContainerDisposed(IEventRegistrationContainer container)
    {
        if (!RegistrationContainerByType.TryRemove(container.EventType, out _))
        {
            this.Logger.LogWarning("Failed to remove disposed event registration container for event type {EventType}.", container.EventType);
        }
    }
}
