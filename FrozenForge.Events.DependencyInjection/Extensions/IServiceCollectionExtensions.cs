using FrozenForge.Events;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
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
}
