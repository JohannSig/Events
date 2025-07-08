FrozenForge.Events is a generic pub/sub system that:

- Lets you register async or sync handlers for typed events.

- Manages event lifecycle using disposables (IDisposable on each registration).

- Automatically removes handlers when their disposable is disposed.

- Supports cancellation via CancellationToken.

- Uses per-event-type containers (EventRegistrationContainer<T>) that dispose themselves when empty.

It’s a robust non-reflection-based, runtime-managed, async-aware messaging system.