using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeOlho.EventBus
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEventSourcing<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
        {
            services.AddScoped<IEventSourcingService>(serviceProvider => 
            {
                return new EventSourcingService(
                    serviceProvider.GetService<TDbContext>());
            });

            services.AddHostedService<EventSourcingBackgroundService<TDbContext>>();

            return services;
        }
    }
}