using System.Threading;
using System.Threading.Tasks;

namespace FrozenForge.Events;

public interface IEventTrigger
{
		Task TriggerAsync<TEvent>(TEvent @event);

		Task TriggerAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
	}
