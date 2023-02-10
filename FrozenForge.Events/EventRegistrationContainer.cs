﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FrozenForge.Events
{
    internal sealed class EventRegistrationContainer<TEvent> : IEventRegistrationContainer<TEvent>
    {
		public event Action<IEventRegistrationContainer> OnDisposed;

		public event Func<TEvent, CancellationToken, Task> OnTrigger;

		public ICollection<IEventRegistration<TEvent>> Registrations { get; } = new List<IEventRegistration<TEvent>>();

		private bool isDisposed;

		public IEventRegistration<TEvent> Register(Func<TEvent, CancellationToken, Task> callback)
        {
			var registration = new EventRegistration<TEvent>();

			OnTrigger += callback;
			registration.OnDisposed += _ => { OnTrigger -= callback; };
			registration.OnDisposed += OnRegistrationDisposed;

			Registrations.Add(registration);

			return registration;
        }

        public Task TriggerAsync(TEvent @event, CancellationToken cancellationToken) => OnTrigger?.Invoke(@event, cancellationToken);

        public void Dispose() => Dispose(true);

        public void Dispose(bool isDisposing)
		{
			if (!isDisposed)
			{
				isDisposed = true;

				if (isDisposing)
				{
					foreach (var registration in Registrations.ToArray())
					{
						registration?.Dispose();
					}

					OnDisposed?.Invoke(this);
				}
			}
		}

		private void OnRegistrationDisposed(IEventRegistration registration)
		{
			registration.OnDisposed -= OnRegistrationDisposed;

			Registrations.Remove(registration as IEventRegistration<TEvent>);

			if (!Registrations.Any())
			{
				Dispose();
			}
		}
	}
}
