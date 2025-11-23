using System;
using System.Collections.Generic;

namespace FrozenForge.Events.Implementations;

public class EventSubscriptionContainer : List<IDisposable>, IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var subscription in this)
        {
            subscription.Dispose();
        }

        this.Clear();
    }
}
