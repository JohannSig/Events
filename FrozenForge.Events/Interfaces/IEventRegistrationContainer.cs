using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrozenForge.Events
{
    internal interface IEventRegistrationContainer : IDisposable
    {
        event Action<IEventRegistrationContainer> OnDisposed;

        Type EventType { get; }
    }

    internal interface IEventRegistrationContainer<TEvent> : IEventRegistrationContainer
    {
        Type IEventRegistrationContainer.EventType => typeof(TEvent);
		
        IEventRegistration<TEvent> Register(Func<TEvent, CancellationToken, Task> eventFunc);

        Task TriggerAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
