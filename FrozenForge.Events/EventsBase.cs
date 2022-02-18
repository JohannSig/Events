using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("FrozenForge.Events.Tests")]
namespace FrozenForge.Events
{

    public class EventsBase : IEvents
    {
        internal IDictionary<Type, IEventRegistrationContainer> RegistrationContainerByType { get; set; } = new ConcurrentDictionary<Type, IEventRegistrationContainer>();

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
				RegistrationContainerByType.Add(typeof(TEvent), container);
            }

			return (container as IEventRegistrationContainer<TEvent>).Register(callback);
        }

		public Task TriggerAsync<TEvent>(TEvent @event) => TriggerAsync(@event, CancellationToken.None);
		
		public Task TriggerAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
			if (RegistrationContainerByType.TryGetValue(typeof(TEvent), out var container))
			{
                return (container as IEventRegistrationContainer<TEvent>).TriggerAsync(@event, cancellationToken);
			}

			return Task.CompletedTask;
        }

        public void Dispose() => Dispose(true);

		public void Dispose(bool isDisposing)
        {
			if (!isDisposed)
            {
				isDisposed = true;

				if (isDisposing)
                {
					foreach (var container in RegistrationContainerByType.Values)
                    {
						container.OnDisposed -= OnRegistrationContainerDisposed;
						container.Dispose();
                    }
                }
            }
        }

		private void OnRegistrationContainerDisposed(IEventRegistrationContainer container)
		{
			RegistrationContainerByType.Remove(container.EventType, out _);
		}
	}
}
