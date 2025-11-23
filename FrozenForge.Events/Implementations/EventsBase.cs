using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("FrozenForge.Events.Tests")]
namespace FrozenForge.Events.Implementations;

public class EventsBase(
    ILoggerFactory loggerFactory) : IEvents
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly ILogger<EventsBase> _logger = loggerFactory.CreateLogger<EventsBase>();
    private readonly ConcurrentDictionary<Type, IEventRegistrationContainer> _registrationContainerByType = [];

    private bool isDisposed;

    public IDisposable Register<TEvent>(Action<TEvent> callback)
        => Register<TEvent>(@event => { callback(@event); return Task.CompletedTask; });

    public IDisposable Register<TEvent>(Func<TEvent, Task> callback)
        => Register<TEvent>((@event, cancellationToken) => callback(@event));

    public IDisposable Register<TEvent>(Func<TEvent, CancellationToken, Task> callback)
    {
        var container = _registrationContainerByType.GetOrAdd(
            typeof(TEvent),
            _ =>
            {
                var container = new EventRegistrationContainer<TEvent>(_loggerFactory.CreateLogger<EventRegistrationContainer<TEvent>>());
                container.OnDisposed += OnRegistrationContainerDisposed;
                return container;
            });

        return ((IEventRegistrationContainer<TEvent>)container).Register(callback);
    }

    public Task TriggerAsync<TEvent>(TEvent @event) => TriggerAsync(@event, CancellationToken.None);

    public Task TriggerAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        if (_registrationContainerByType.TryGetValue(typeof(TEvent), out var container))
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
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;

        if (isDisposing)
        {
            foreach (var containerKey in _registrationContainerByType.Keys.ToList())
            {
                if (!_registrationContainerByType.TryRemove(containerKey, out var container))
                {
                    this._logger.LogWarning("Failed to remove event registration container for event type {EventType} during disposal.", containerKey);
                    continue;
                }

                container.OnDisposed -= OnRegistrationContainerDisposed;
                container.Dispose();
            }
        }
    }

    private void OnRegistrationContainerDisposed(IEventRegistrationContainer container)
    {
        if (!_registrationContainerByType.TryRemove(container.EventType, out _))
        {
            this._logger.LogWarning("Failed to remove disposed event registration container for event type {EventType}.", container.EventType);
        }
    }
}
