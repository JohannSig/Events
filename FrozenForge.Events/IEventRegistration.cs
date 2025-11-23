using System;

namespace FrozenForge.Events;

internal interface IEventRegistration : IDisposable
{
    event Action<IEventRegistration> OnDisposed;
}

internal interface IEventRegistration<TEvent> : IEventRegistration
{
}
