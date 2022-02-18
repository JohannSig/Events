using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrozenForge.Events
{
    internal interface IEventRegistration : IDisposable
    {
        event Action<IEventRegistration> OnDisposed;
    }

    internal interface IEventRegistration<TEvent> : IEventRegistration
    {
    }
}
