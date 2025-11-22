using System;
using System.Collections.Generic;

namespace FrozenForge.Events.Implementations;

public class EventSubscriptionContainer : List<IDisposable>, IDisposable
{
    public EventSubscriptionContainer()
        : base()
    {

    }

    public void Dispose()
    {
        foreach (var subscription in this)
        {
            subscription.Dispose();
        }

        this.Clear();
    }
}
