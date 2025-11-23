using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FrozenForge.Events.Tests")]
namespace FrozenForge.Events;

public interface IEvents : IEventListener, IEventTrigger
{

}
