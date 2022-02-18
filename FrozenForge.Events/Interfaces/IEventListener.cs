using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrozenForge.Events
{
    public interface IEventListener : IDisposable
    {
		IDisposable Register<TEvent>(Action<TEvent> callback);

		IDisposable Register<TEvent>(Func<TEvent, Task> callback);

		IDisposable Register<TEvent>(Func<TEvent, CancellationToken, Task> callback);
	}
}
