using System;

namespace FrozenForge.Events
{
    internal class EventRegistration<TEvent> : IEventRegistration<TEvent>
	{
		public event Action<IEventRegistration>? OnDisposed;

        public void Dispose()
        {
            OnDisposed?.Invoke(this);
        }
    }
}
