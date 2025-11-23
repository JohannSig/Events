using FrozenForge.Events;
using FrozenForge.Events.Implementations;
using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services, ServiceLifetime serviceLifetime)
    {
        return serviceLifetime switch
        {
            ServiceLifetime.Scoped => services.AddScopedEvents(),

            ServiceLifetime.Singleton => services.AddSingletonEvents(),

            _ => throw new ArgumentOutOfRangeException($"{serviceLifetime} event configuration is not supported"),
        };
    }

    public static IServiceCollection AddScopedEvents(this IServiceCollection services)
    {
        return services
            .AddScoped<IEvents, EventsBase>()
            .AddScoped<IEventListener>(services => services.GetRequiredService<IEvents>())
            .AddScoped<IEventTrigger>(services => services.GetRequiredService<IEvents>());
    }

    public static IServiceCollection AddSingletonEvents(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEvents, EventsBase>()
            .AddSingleton<IEventListener>(services => services.GetRequiredService<IEvents>())
            .AddSingleton<IEventTrigger>(services => services.GetRequiredService<IEvents>());
    }
}
