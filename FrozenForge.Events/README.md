# FrozenForge.Events

FrozenForge.Events is a small, thread-safe, async-aware in-process pub/sub system for .NET 8 that uses generic event types and disposable registrations to manage lifecycle without reflection.

## Key concepts (implementation overview)
- Per-event-type containers:
  - Each event type `TEvent` gets a dedicated `EventRegistrationContainer<TEvent>`.
  - Containers are created on first registration by `EventsBase` and stored in a `ConcurrentDictionary<Type, IEventRegistrationContainer>`.
  - Containers raise `OnDisposed` when they are disposed so `EventsBase` can remove them from the dictionary.

- Registration objects:
  - Register handlers via `IEventListener` (`EventsBase` implements `IEvents`, which inherits `IEventListener` and `IEventTrigger`).
  - Supported registration signatures:
    - `Register<TEvent>(Action<TEvent>)`
    - `Register<TEvent>(Func<TEvent, Task>)`
    - `Register<TEvent>(Func<TEvent, CancellationToken, Task>)`
  - Each registration returns an `IDisposable` (an `EventRegistration<TEvent>`) that, when disposed, removes its callback from the container. `EventRegistration` raises an `OnDisposed` event to drive removal logic.

- Triggering events:
  - Trigger events via `IEventTrigger`:
    - `TriggerAsync<TEvent>(TEvent @event)`
    - `TriggerAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)`
  - `EventRegistrationContainer<TEvent>` captures the current callbacks and invokes them concurrently using `Task.WhenAll`. Callbacks receive the event instance and a `CancellationToken`.

- Concurrency and safety:
  - Uses `ConcurrentDictionary` and thread-safe patterns to allow concurrent registration/unregistration and triggering.
  - Removing a registration is safe during triggering; the container snapshots keys before invoking callbacks.
  - Containers and registrations implement `IDisposable` and properly clean up to avoid leaks.

- Logging:
  - `EventsBase` and `EventRegistrationContainer<TEvent>` accept `ILoggerFactory` / `ILogger<T>` and log warnings on unexpected conditions (e.g., triggering on disposed containers, failed removals).

## Typical usage
- Create an `EventsBase` with an `ILoggerFactory`.
- Register handlers and keep the returned `IDisposable`.
- Trigger events asynchronously.
- Dispose registrations or the `EventsBase` to clean up.

Example:
````````
```csharp
// using FrozenForge.Events;

var events = new EventsBase(new LoggerFactory());

// Registering a handler synchronously
var registration1 = events.Register<string>(s => Console.WriteLine($"Handler 1: {s}"));

// Registering a handler asynchronously
var registration2 = events.Register<string>(async (s, ct) => {
    await Task.Delay(100);
    Console.WriteLine($"Handler 2: {s}");
});

// Triggering the event
await events.TriggerAsync("Hello, Events!");

registration1.Dispose(); // Disposing the first handler registration

// This will only trigger the second handler
await events.TriggerAsync("Hello again, Events!");

events.Dispose(); // Disposing the EventsBase also cleans up all registrations
````````

````````markdown

## Design notes
- No reflection: type-safety is provided by generics and explicit per-type containers.
- Disposal-driven lifecycle: individual handlers are removed by disposing their registration; empty containers dispose and notify `EventsBase`.
- Optimized for simple in-process messaging: lightweight, predictable, and suitable for domain events, UI events, or simple integration points.

## Project files of interest
- `EventsBase` – central manager (`IEvents`) creating and removing per-type containers.
- `EventRegistrationContainer<TEvent>` – holds callbacks and invokes them concurrently.
- `EventRegistration<TEvent>` – represents a single registration; raises disposal notifications.
- `EventSubscriptionContainer` – helper to manage groups of `IDisposable` subscriptions.