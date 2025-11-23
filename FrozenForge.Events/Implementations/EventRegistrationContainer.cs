using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrozenForge.Events.Exceptions;
using Microsoft.Extensions.Logging;

namespace FrozenForge.Events.Implementations;

internal sealed class EventRegistrationContainer<TEvent>(ILogger<EventRegistrationContainer<TEvent>> logger) : IEventRegistrationContainer<TEvent>
{
    public event Action<IEventRegistrationContainer>? OnDisposed;

    // Map each registration to its callback so we can run them concurrently.
    private readonly ConcurrentDictionary<IEventRegistration<TEvent>, Func<TEvent, CancellationToken, Task>> _callbackByRegistration = [];

    private readonly ILogger<EventRegistrationContainer<TEvent>> _logger = logger;
    
    private bool isDisposed;

    public IEventRegistration<TEvent> Register(Func<TEvent, CancellationToken, Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(EventRegistrationContainer<TEvent>), "Cannot register callback on disposed container.");
        }

        var registration = new EventRegistration<TEvent>();

        if (!_callbackByRegistration.TryAdd(registration, callback))
        {
            // This should never happen, but just in case.
            throw new EventRegistrationException("Failed to add callback for new registration.");
        }

        // Remove mapping when this registration is disposed.
        registration.OnDisposed += _ =>
        {
            if (!_callbackByRegistration.TryRemove(registration, out var _) && !isDisposed)
            {
                this._logger.LogWarning("Failed to remove callback for disposed registration.");
            }            
        };

        // Keep existing centralized disposal handling.
        registration.OnDisposed += OnRegistrationDisposed;

        return registration;
    }

    public Task TriggerAsync(TEvent @event, CancellationToken cancellationToken)
    {
        if (isDisposed)
        {
            this._logger.LogWarning("Attempted to trigger event on disposed registration container.");

            return Task.CompletedTask;
        }

        var keys = _callbackByRegistration.Keys.ToHashSet();

        var callbackTasks = keys
            .Select(k => !_callbackByRegistration.TryGetValue(k, out var callback) ? null : callback)
            .OfType<Func<TEvent, CancellationToken, Task>>()
            .Select(callback => callback.Invoke(@event, cancellationToken))
            .ToArray();

        return Task.WhenAll(callbackTasks);
    }

    public void Dispose() => Dispose(true);

    public void Dispose(bool isDisposing)
    {
        if (!isDisposed)
        {
            isDisposed = true;

            if (isDisposing)
            {
                while (!_callbackByRegistration.IsEmpty)
                {
                    var registration = _callbackByRegistration.Keys.FirstOrDefault();

                    if (registration is null)
                    {
                        continue;
                    }

                    if (!_callbackByRegistration.TryRemove(registration, out _))
                    {
                        _logger.LogWarning("Failed to remove callback for disposed registration.");
                    }

                    registration?.Dispose();
                }

                OnDisposed?.Invoke(this);
            }
        }
    }

    private void OnRegistrationDisposed(IEventRegistration registration)
    {
        // Unsubscribe this handler first to avoid reentrancy.
        registration.OnDisposed -= OnRegistrationDisposed;

        if (!_callbackByRegistration.TryRemove((IEventRegistration<TEvent>)registration, out _) && !isDisposed)
        {
            _logger.LogWarning("Failed to remove callback for disposed registration.");
        }
    }
}
